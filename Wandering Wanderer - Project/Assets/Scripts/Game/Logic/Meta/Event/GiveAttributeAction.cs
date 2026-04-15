namespace Game.Logic.Meta.Event
{
    /// <summary>
    /// Cộng điểm vào availableAttributePoints.
    /// Player phân bổ điểm qua AttributeAllocationView.
    /// </summary>
    public class GiveAttributeAction
    {
        public int Amount { get; private set; }
        
        public GiveAttributeAction(int amount)
        {
            Amount = amount;
        }
    }
}