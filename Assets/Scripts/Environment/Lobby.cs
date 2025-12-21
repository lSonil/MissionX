using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    public static Lobby i;
    public Transform playerSpawnPoint;
    public Transform playerPrefab;
    public PlayerCore[] players;
    public List<NPCEntry> possibleNPC = new List<NPCEntry>();

    public List<BuffState> globalBuffs = new List<BuffState>();
    public List<DebuffState> globalDebuffs = new List<DebuffState>();
    
    private void Awake()
    {
        if (i != null && i != this)
        {
            Destroy(gameObject);
            return;
        }

        i = this;

        DontDestroyOnLoad(gameObject);
        SceneData.AssignMonstersToMissions(possibleNPC);
    }

    private void Start()
    {
        players = FindObjectsByType<PlayerCore>(FindObjectsSortMode.None);

        foreach (PlayerCore p in players)
            p.uis.endOfMissionCamera.SetActive(false);

        if(players.Length==0)
        {
            Transform player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            player.transform.SetParent(transform);
        }
        
    }

    public void SetBuffsAndDebuffs(Dictionary<string, ContainedState> results, MissionData rewards)
    {
        bool allFree = true;
        bool anyFree = false;

        foreach (var entry in results)
        {
            if (entry.Value == ContainedState.Free)
                anyFree = true;
            else
                allFree = false;
        }

        if (!anyFree)
        {
            foreach (var buff in rewards.buffs)
            {
                BuffState existing = globalBuffs.Find(b => b.buff == buff);

                if (existing != null)
                {
                    existing.times += 1;
                }
                else
                {
                    globalBuffs.Add(new BuffState
                    {
                        buff = buff,
                        times = 1
                    });
                }
            }

            return;
        }
        else
        if (allFree)
        {
            foreach (var debuff in rewards.debuffs)
            {
                DebuffState existing = globalDebuffs.Find(d => d.debuff == debuff);

                if (existing != null)
                {
                    existing.times += 1;
                }
                else
                {
                    globalDebuffs.Add(new DebuffState
                    {
                        debuff = debuff,
                        times = 1
                    });
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
