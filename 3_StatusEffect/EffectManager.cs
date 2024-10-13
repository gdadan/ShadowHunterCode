using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    public List<GameObject> particles = new List<GameObject>(); //파티클 이펙트

    public List<GameObject> hitEffects = new List<GameObject>(); // Key: 100~
   // public List<GameObject> 

    Dictionary<int, GameObject> statusEffectPrefab = new Dictionary<int, GameObject>();
    Dictionary<int, Queue<ParticleSystem>> particlePool = new Dictionary<int, Queue<ParticleSystem>>();

    private void Awake()
    {
        Instance = this;

        Init();
    }

    public ParticleSystem GetParticle(int type)
    {
        ParticleSystem particleObj = ObjectPoolManager.GetPooledObject(particlePool[type]);

        // 풀에 없을 경우
        if (particleObj == null)
        {
            if (type < 100)
            { particleObj = Instantiate(statusEffectPrefab[type], transform).GetComponent<ParticleSystem>(); }
            else
            {
                int index = type % 100;
                particleObj = Instantiate(hitEffects[index-1], transform).GetComponent<ParticleSystem>();
            }
        }

        particleObj.gameObject.SetActive(true);

        return particleObj.GetComponent<ParticleSystem>();
    }

    //사용한 파티클 풀에 반환
    public void ReturnParticle(int type, ParticleSystem particleObj)
    {
        particleObj.gameObject.SetActive(false);
        ObjectPoolManager.ReturnPooledObject(particlePool[type], particleObj);
        particleObj.transform.SetParent(transform);
    }

    void Init()
    {
        List<int> keys = new List<int>();

        for (int i = 0;i < particles.Count;i++)
        {
            string name = particles[i].name;
            int key = int.Parse(name.Split('_')[0]);
            statusEffectPrefab.Add(key, particles[i]);

            keys.Add(key);
        }

        for(int j=0; j< hitEffects.Count; j++)
        {
            keys.Add(101 + j);
        }

        ObjectPoolManager.CreatePoolQueue(particlePool, keys);
    }
}