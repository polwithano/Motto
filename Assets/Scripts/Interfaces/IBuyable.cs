namespace Interfaces
{
    public interface IBuyable
    {
        void ProcessPurchase(); 
        void SetPrice(uint price);
        public uint DefaultValue {get; set;}
    }
}