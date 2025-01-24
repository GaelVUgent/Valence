namespace MICT.eDNA.Interfaces
{
    public interface IStateService
    {
    }

    public enum Phase { 
        Introduction,
        Tutorial,
        Test,
        Conclusion
    }

    public class Interactable {
        public string Name;

    }
}