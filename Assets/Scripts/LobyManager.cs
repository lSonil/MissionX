using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobyManager : MonoBehaviour
{
    public static LobyManager i;
    public List<NPCEntry> possibleNPC = new List<NPCEntry>();
    public GameObject missionManagerPrefab;
    GameObject missionManager;
    public int selectedMission = -1;
    public Terminal terminalToUse;
    public Transform endCameraPos;
    public Transform playerSpawnPoint;
    public bool inMission = false;
    private List<ContainedState> previousStates = new List<ContainedState>();
    public List<BuffState> globalBuffs = new List<BuffState>();
    public List<DebuffState> globalDebuffs = new List<DebuffState>();
    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        SceneData.AssignMonstersToMissions(possibleNPC);
        terminalToUse.ShowMainMenu();
    }
    public IEnumerator LoadMision()
    {
        yield return new WaitForSeconds(1f);

        if (!inMission)
        {
            yield return new WaitForSeconds(1f);

            missionManager = Instantiate(missionManagerPrefab, Vector3.zero, Quaternion.identity);

            while (SceneData.npcInScene == null)
            {
                if (missionManager == null)
                    missionManager = Instantiate(missionManagerPrefab, Vector3.zero, Quaternion.identity);
                yield return null;
            }
            StartCoroutine(MonitorContainmentStates());
            StartCoroutine(MonitorPlayerStates());
            terminalToUse.ShowMainMenu();
        }
        else
        {
            SceneData.DayEnd();
            selectedMission = -1;

            yield return new WaitForSeconds(2f);

            foreach (PlayerCore p in LobyData.players)
            {
                p.hs.error.SetActive(false);
                p.Stop(endCameraPos, transform);
            }

            yield return new WaitForSeconds(3f);

            SetBuffsAndDebuffs(SceneData.containmentResults, SceneData.missionToTransfer);

            foreach (PlayerCore p in LobyData.players)
            {
                StartCoroutine(p.uis.ShowResults());
                p.GetReady(playerSpawnPoint);
            }
            Destroy(missionManager.gameObject);

            SceneData.AssignMonstersToMissions(possibleNPC);
            terminalToUse.ShowMainMenu();

        }
        inMission = !inMission;
    }
    private IEnumerator MonitorContainmentStates()
    {
        bool print = true;

        while (SceneData.npcInScene == null)
        {
            yield return null;
        }

        previousStates = Enumerable.Repeat(ContainedState.Free, SceneData.npcInScene.Count).ToList();

        while (selectedMission != -1)
        {
            for (int i = 0; i < SceneData.npcInScene.Count; i++)
            {
                var current = SceneData.npcInScene[i].contained;
                var previous = previousStates[i];

                if (current != previous)
                {
                    string name = SceneData.missionToTransfer.monsters[i].id;

                    if (previous == ContainedState.Contained && current == ContainedState.Free)
                    {
                        foreach (PlayerCore p in LobyData.players)
                            p.uis.popUpHanle.ShowPopUp("r", name);
                        print = true;
                    }
                    else if (previous == ContainedState.Free && (current == ContainedState.Contained || current == ContainedState.Suppressed))
                    {
                        foreach (PlayerCore p in LobyData.players)

                            p.uis.popUpHanle.ShowPopUp("y", name);
                        print = true;
                    }

                    previousStates[i] = current;
                }
            }

            if (SceneData.npcInScene.All(n => n.contained == ContainedState.Contained || n.contained == ContainedState.Suppressed) && print)
            {
                print = false;
                foreach (PlayerCore p in LobyData.players)
                    p.uis.popUpHanle.ShowPopUp("g", "All contained");
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
    private IEnumerator MonitorPlayerStates()
    {
        bool allFine = true;
        while (allFine)
        {
            foreach (PlayerCore p in LobyData.players)
            {
                if (p.hs.IsDead())
                {
                    allFine = false;
                    break;
                }
                yield return null;
            }
        }
        terminalToUse.Go();
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
        else if (allFree)
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
}