namespace System.IO.Ports.Net40;

public class SerialDataReceivedEventArgs : EventArgs
{
	public SerialData EventType { get; private set; }

	internal SerialDataReceivedEventArgs(SerialData eventCode)
	{
		EventType = eventCode;
	}
}
