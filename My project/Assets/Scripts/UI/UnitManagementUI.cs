using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnitStats))]
public class UnitManagementUI : MonoBehaviour
{
    private UnitStats unitStats;
    private Rect windowRect = new Rect(10, 10, 350, 600);
    private Vector2 scrollPosition;

    private void Awake()
    {
        unitStats = GetComponent<UnitStats>();
        if (unitStats == null)
            Debug.LogError("UnitManagementUI requires a UnitStats component.", this);
    }

    public bool IsPointerOverWindow()
    {
        if (Mouse.current == null)
            return false;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 pointerPosition = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
        return windowRect.Contains(pointerPosition);
    }

    private void OnGUI()
    {
        if (unitStats == null)
            return;

        windowRect = GUI.Window(52731, windowRect, DrawWindow, "유닛 관리");
    }

    private void DrawWindow(int windowId)
    {
        TurnManager turnManager = TurnManager.Instance;
        if (turnManager != null)
        {
            GUILayout.Label(turnManager.PlayerTurn ? "아군 턴" : "적 턴 (행동 없음)");
            if (turnManager.PlayerTurn && GUILayout.Button("턴 종료"))
                turnManager.EndPlayerTurn();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label($"체력 {unitStats.currentHealth}/{unitStats.health} | 공격력 {unitStats.EffectiveAttackPower} | 방어력 {unitStats.EffectiveDefense}");
        GUILayout.Label($"이동 거리 {unitStats.EffectiveMoveRange} | 공격 거리 {unitStats.attackRange} | 치명타 {unitStats.criticalChance:P0}");
        GUILayout.Space(8);
        GUILayout.Label("능력치 강화 (체력 +10, 그 외 +1 / 치명타 +5%)");
        DrawUpgradeButton("체력 강화", UnitUpgradeType.Health);
        DrawUpgradeButton("공격력 강화", UnitUpgradeType.AttackPower);
        DrawUpgradeButton("방어력 강화", UnitUpgradeType.Defense);
        DrawUpgradeButton("이동 거리 강화", UnitUpgradeType.MoveRange);
        DrawUpgradeButton("공격 거리 강화", UnitUpgradeType.AttackRange);
        DrawUpgradeButton("치명타 확률 강화", UnitUpgradeType.CriticalChance);

        GUILayout.Space(8);
        GUILayout.Label("보유 장비");
        for (int i = 0; i < unitStats.Inventory.Count; i++)
        {
            UnitEquipment item = unitStats.Inventory[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{item.itemName} ({item.GetBonusDescription()})");
            if (GUILayout.Button(unitStats.IsEquipped(i) ? "해제" : "장착", GUILayout.Width(50)))
                unitStats.EquipItem(i);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(8);
        GUILayout.Label("강화 및 장착 이력");
        for (int i = unitStats.ProgressionHistory.Count - 1; i >= 0; i--)
        {
            UnitProgressionRecord record = unitStats.ProgressionHistory[i];
            GUILayout.Label($"{record.timestamp}  {record.description}");
        }

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void DrawUpgradeButton(string label, UnitUpgradeType upgradeType)
    {
        if (GUILayout.Button(label))
            unitStats.Upgrade(upgradeType);
    }
}
