public class Affector<T> {

    /// <summary>
    /// Unique ID for an Affector
    /// For example: World1_forestry_coin
    /// </summary>
    private string ID;

    /// <summary>
    /// How much affection does this Affector produce
    /// </summary>
    private T affection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// 
    /// Constructs a Affection with an ID and the Template var T
    /// 
    /// <param name="ID"></param>
    /// <param name="affection"></param>
    public Affector(string ID, T affection) {
        this.ID = ID;
        this.affection = affection;
    }

    /// <summary>
    /// Returns the unique ID of the Affector
    /// </summary>
    /// <returns></returns>
    public string getID() {
        return ID;
    }

    /// <summary>
    /// returns the affection of the affector
    /// </summary>
    /// <returns>affection</returns>
    public T getAffection() {
        return affection;
    }

    /// <summary>
    /// sets the affection
    /// </summary>
    /// <param name="newIncome"></param>
    public void setAffection(T newAffection) {
        affection = newAffection;
    }
}