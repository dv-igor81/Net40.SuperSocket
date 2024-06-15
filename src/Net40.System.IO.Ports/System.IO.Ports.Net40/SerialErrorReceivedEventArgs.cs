namespace System.IO.Ports.Net40;

public class SerialErrorReceivedEventArgs : EventArgs
{
	public SerialError EventType { get; private set; }

	internal SerialErrorReceivedEventArgs(SerialError eventCode)
	{
		EventType = eventCode;
	}
}
