namespace Interfaces
{
    public interface IEffectEmitter
    {
        public string ID { get; set; }
        public bool IsInstance(string id); 
    }
}