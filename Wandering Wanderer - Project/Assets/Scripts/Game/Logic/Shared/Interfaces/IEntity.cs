namespace Game.Logic.Shared
{
    /// <summary>
    /// Contract chung cho tất cả Entity (Player, Enemy).
    /// Cho phép các hệ thống như DamageCalculator hay SpellEffect truy cập các thuộc tính cần thiết.
    /// </summary>
    public interface IEntity
    {
        int CurrentHp { get; set; }
        int MaxHp { get; }

        bool HasEffect(EffectType effectType);
        float GetEffectivePotency(Element element);
        float GetEffectiveResistance(Element element);
        float GetEffectiveAGI();
    }
}
