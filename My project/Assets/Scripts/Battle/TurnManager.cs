using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public bool PlayerTurn = true;
    public bool EnemyTurn = false;
    public float enemyTurnDuration = 0.5f;

    public static TurnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        SetTurn(Team.Player);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void EndPlayerTurn()
    {
        if (!PlayerTurn)
            return;

        SetTurn(Team.Enemy);
        ResetTeamActions(Team.Enemy);
        StartCoroutine(PassEnemyTurn());
    }

    private IEnumerator PassEnemyTurn()
    {
        yield return new WaitForSeconds(enemyTurnDuration);
        BeginPlayerTurn();
    }

    private void BeginPlayerTurn()
    {
        ResetTeamActions(Team.Player);
        SetTurn(Team.Player);
    }

    private void ResetTeamActions(Team team)
    {
        UnitStats[] units = FindObjectsByType<UnitStats>(FindObjectsSortMode.None);
        foreach (UnitStats unit in units)
        {
            if (unit.team != team)
                continue;

            unit.hasMove = false;
            unit.hasActed = false;
        }
    }

    private void SetTurn(Team team)
    {
        PlayerTurn = team == Team.Player;
        EnemyTurn = team == Team.Enemy;
        Debug.Log(PlayerTurn ? "플레이어 턴" : "적 턴");
    }
}
