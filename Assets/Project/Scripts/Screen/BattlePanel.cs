using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
    public Slider HealthSlider;
    public Text HealthText;
    public Slider EXPSlider;
    public Text EXPText;
    public Text LevelText;
    public Text CoinText;
    public Text EnemyCount;

    public Image RecoverMask;

    public Image HeavyAttackMask;   
    public Image BurnSkillMask;    

    public Button PauseBtn;

    private void Update()
    {
        UpdateRecover();
        UpdateHeavyAttack();
        UpdateBurnSkill();
    }

    public override void Show()
    {
        base.Show();
        UpdatePanel();

        if (PauseBtn != null)
        {
            PauseBtn.onClick.RemoveAllListeners();
            PauseBtn.onClick.AddListener(() =>
            {
                UIService.Instance.ShowPanel<PausePanel>(3);
            });
        }
    }

    public override void UpdatePanel()
    {
        base.UpdatePanel();

        float curHp = PlayerSystem.Instance.CurrentHeathl;
        float maxHp = PlayerSystem.Instance.MaxHeathl;

        if (HealthSlider != null)
            HealthSlider.value = maxHp > 0 ? curHp / maxHp : 0;

        if (HealthText != null)
            HealthText.text = $"{curHp}/{maxHp}";

        if (EXPSlider != null)
            EXPSlider.value = PlayerSystem.Instance.CurrentEXP / 100f;

        if (EXPText != null)
            EXPText.text = PlayerSystem.Instance.CurrentEXP + "/100";

        if (LevelText != null)
            LevelText.text = PlayerSystem.Instance.Level.ToString();

        if (CoinText != null)
            CoinText.text = PlayerSystem.Instance.CoinCount.ToString();

        if (EnemyCount != null)
            EnemyCount.text = LevelSystem.Instance.EnemyCount.ToString();

        UpdateRecover();
        UpdateHeavyAttack();
        UpdateBurnSkill();
    }

    public void UpdateRecover()
    {
        if (RecoverMask != null)
        {
            RecoverMask.fillAmount =
                PlayerSystem.Instance.RecoverCD / PlayerSystem.Instance.BaseRecoverCD;
        }
    }

    public void UpdateHeavyAttack()
    {
        if (HeavyAttackMask == null) return;

        Player player = PlayerSystem.Instance.player;
        if (player == null) return;

        HeavyAttackMask.fillAmount = 1f - player.GetGunSkillCDPercent();
    }

    public void UpdateBurnSkill()
    {
        if (BurnSkillMask == null) return;

        Player player = PlayerSystem.Instance.player;
        if (player == null) return;

        BurnSkillMask.fillAmount = 1f - player.GetBurnSkillCDPercent();
    }
}
