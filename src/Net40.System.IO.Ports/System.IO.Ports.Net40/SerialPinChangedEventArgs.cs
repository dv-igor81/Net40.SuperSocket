namespace System.IO.Ports.Net40;

public class SerialPinChangedEventArgs : EventArgs
{
	public SerialPinChange EventType { get; private set; }

	internal SerialPinChangedEventArgs(SerialPinChange eventCode)
	{
		EventType = eventCode;
	}
}
