namespace Game {

public interface IMotor {
    bool Reachable(DVector3 position);
    bool MoveTo(DVector3 position);
    void Stop();
}

}
