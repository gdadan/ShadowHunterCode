# 섀도우 헌터 키우기 코드

회사 프로젝트에서 작업한 코드입니다.

> *상업적 목적이 아닌 포트폴리오용으로 허락받고 공개하고 있습니다.

<br/>

## 📝 프로젝트 소개

| 항목 | 내용 |
|------|------|
| **개발 기간** | 2024.05 ~ 2024.09 (5개월) |
| **개발 환경** | C#, Unity 2022.3 LTS |
| **개발 장르** | 2D 모바일 방치형 RPG |
| **담당 영역** | 스킬 시스템 / 펫 시스템 |

**한 줄 설명** : 변신을 활용하여 다양한 속성의 필드에서 전투를 하며 캐릭터를 성장시키는 모바일 방치형 RPG입니다.

<br/>

## 📷 인게임 화면

<img src="https://github.com/user-attachments/assets/8ff88eee-d458-4116-83f3-dc48b53ea96b" width="20%" />
<img src="https://github.com/user-attachments/assets/59db6c8d-2b1e-4e84-a0bd-ec3e9df42385" width="20%" />
<img src="https://github.com/user-attachments/assets/0a4fd97d-8dfc-4064-bc10-b6c92db70617" width="20%" />

<br/>

## 🎯 핵심 작업 요약

| 영역 | 규모 |
|------|------|------------|
| 스킬 시스템 | 6개 캐릭터 / **40개 스킬**  |
| 상태이상 시스템 | **9종 효과** (디버프 6 / 버프 3) |
| 펫 시스템 | UI / AI  |

<br/>

---

## 🛠 1. 스킬 시스템 — 40개 스킬을 일관된 규약으로 확장하기

### 무엇을 (What)
6개 캐릭터별 **총 40개의 액티브 스킬**과 **레벨 10/20/30 단계별 강화 효과**를 구현했습니다.

### 왜 (Why)
- 스킬마다 발동 방식(논타겟 / 가장 가까운 적 / 다중 타겟), 지속 형태(즉발 / 범위 지속 / 투사체)가 모두 달라, 매번 분기 처리하면 **40개의 if/switch 지옥**이 됨
- 신규 스킬 추가 시 기존 매니저 코드를 수정해야 하는 구조 → 기획 변경에 매우 취약

### 어떻게 (How)
**추상 클래스 + 인터페이스 + 데이터 분리**의 3단 구조로 설계했습니다.

```csharp
// 1. 모든 스킬의 공통 규약 (Skill 추상)
public abstract class Skill : MonoBehaviour
{
    public abstract void UseSkill();
    public abstract void CheckSkillUpgrade();
}

// 2. 액티브 스킬 공통 동작 (ActiveSkill 추상)
public abstract class ActiveSkill : Skill
{
    public abstract void UseActiveSkill();
    public abstract void AddFirstUpgrade();   // 10레벨 강화 (강제)
    public abstract void AddSecondUpgrade();  // 20레벨 강화 (강제)
    public abstract void AddThirdUpgrade();   // 30레벨 강화 (강제)
}

// 3. 타겟팅 스킬은 인터페이스로 분리
public interface ITargetingSkill { List<Transform> targetList { get; set; } }
public interface IDamageable { void OnDamaged(float damage, bool isCri); }
```

- **OCP(개방-폐쇄 원칙) 준수** : 신규 스킬 추가 시 `ActiveSkill`을 상속한 클래스만 추가하면 끝. 기존 매니저/엔진 코드 수정 0줄.
- **공통 로직은 부모에서 일괄 처리** : 데미지 계산, 레벨업 시 능력치 증가, 콜라이더 레이어 변경(`SetSkillLayer`)을 부모가 담당
- **확장 가능 인터페이스** : 타겟팅이 필요한 스킬만 `ITargetingSkill` 구현 → 단일 책임 분리

🔗 [스킬 추상 클래스 (SkillClass.cs)](./1_Class/SkillClass.cs)
🔗 [스킬 구현체 40종](./2_SkillScripts)

<br/>

---

## ⚡ 2. 상태이상 시스템 — 전략 패턴(Strategy Pattern)으로 9종 효과 통합

### 무엇을 (What)
기절 / 공중부양 / 넉백 / 빙결 / 화상 / 출혈 / 공격력↑ / 크리티컬↑ / 가속 — **9가지 상태이상**을 구현하고, **중첩·갱신·만료 처리**를 일관되게 관리하는 매니저를 만들었습니다.

### 왜 (Why)
- 같은 적에게 화상 + 출혈 + 빙결이 동시에 들어올 때, 각 효과마다 시간 갱신·중첩 가능 여부가 다름
- 효과별로 코드를 흩뿌리면 **중복 적용 / 해제 누락 / 메모리 누수** 등 버그가 폭발적으로 늘어남

### 어떻게 (How)
`StatusEffect` 추상 클래스를 정의하고, 각 효과를 동일 인터페이스로 추상화 — **전략 패턴**을 적용했습니다.

```csharp
public abstract class StatusEffect
{
    public bool isDuplicated;   // 중첩 가능 여부
    public float duration;
    public abstract void Active();
    public abstract void Remove();
}

// 매니저는 효과의 종류를 몰라도 일관되게 처리
public void AddBuff(StatusEffect statusEffect)
{
    var existing = activeBuffs.Find(b =>
        b.target == statusEffect.target &&
        b.statusEffectType == statusEffect.statusEffectType);

    if (existing != null && existing.isDuplicated)
        existing.duration += statusEffect.duration;  // 시간 누적
    else
        StartCoroutine(EffectCoroutine(statusEffect));
}
```

- **단일 진입점** : `AddBuff()` 하나로 9종 효과 모두 처리. 매니저는 구체 타입을 몰라도 됨
- **중첩 정책 일원화** : `isDuplicated` 플래그로 효과별 정책을 데이터처럼 표현
- **자원 누수 방지** : 코루틴 종료 시 `Remove()` → 파티클 풀 반환 → 리스트 제거가 자동 흐름

🔗 [상태효과 클래스 (StatusEffect.cs)](./3_StatusEffect/StatusEffect.cs)
🔗 [매니저 (StatusEffectManager.cs)](./3_StatusEffect/StatusEffectManager.cs)

<br/>

---


## 💡 회고 — 이 프로젝트에서 배운 것

- **추상화의 가치** : 처음 스킬을 만들 때는 if/switch로도 충분했지만, 수가 점점 많아지니 추상 클래스 없이는 유지보수가 어려웠습니다. **확장될 영역에 미리 추상화를 두고 생각하는 감각**을 익혔습니다.

<br/>

