/// Refused is 0 so an unset value refuses by default - the safe direction,
/// since the failure mode of the other default is giving away a 34-coin
/// Cappuccino for nothing.
public enum ServeResponse
{
    Refused = 0,
    Accepted = 1,
    Preferred = 2
}