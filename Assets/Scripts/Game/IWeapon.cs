namespace Game {

public interface IWeapon {
    DReal Range();
    void FireAt(Entity target);
}

}
