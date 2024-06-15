using System.ComponentModel;
using System.Text;
using Microsoft.Win32;

namespace System.IO.Ports.Net40;

public class SerialPort : Component
{
	public const int InfiniteTimeout = -1;

	private const int DefaultDataBits = 8;

	private const Parity DefaultParity = Parity.None;

	private const StopBits DefaultStopBits = StopBits.One;

	private const Handshake DefaultHandshake = Handshake.None;

	private const int DefaultBufferSize = 1024;

	private const string DefaultPortName = "COM1";

	private const int DefaultBaudRate = 9600;

	private const bool DefaultDtrEnable = false;

	private const bool DefaultRtsEnable = false;

	private const bool DefaultDiscardNull = false;

	private const byte DefaultParityReplace = 63;

	private const int DefaultReceivedBytesThreshold = 1;

	private const int DefaultReadTimeout = -1;

	private const int DefaultWriteTimeout = -1;

	private const int DefaultReadBufferSize = 4096;

	private const int DefaultWriteBufferSize = 2048;

	private const int MaxDataBits = 8;

	private const int MinDataBits = 5;

	private const string DefaultNewLine = "\n";

	private const string SERIAL_NAME = "\\Device\\Serial";

	private int _baudRate = 9600;

	private int _dataBits = 8;

	private Parity _parity;

	private StopBits _stopBits = StopBits.One;

	private string _portName = "COM1";

	private Encoding _encoding = Encoding.ASCII;

	private Decoder _decoder = Encoding.ASCII.GetDecoder();

	private int _maxByteCountForSingleChar = Encoding.ASCII.GetMaxByteCount(1);

	private Handshake _handshake;

	private int _readTimeout = -1;

	private int _writeTimeout = -1;

	private int _receivedBytesThreshold = 1;

	private bool _discardNull;

	private bool _dtrEnable;

	private bool _rtsEnable;

	private byte _parityReplace = 63;

	private string _newLine = "\n";

	private int _readBufferSize = 4096;

	private int _writeBufferSize = 2048;

	private SerialStream _internalSerialStream;

	private byte[] _inBuffer = new byte[1024];

	private int _readPos;

	private int _readLen;

	private char[] _oneChar = new char[1];

	private char[] _singleCharBuffer;

	private SerialDataReceivedEventHandler _dataReceivedHandler;

	private SerialDataReceivedEventHandler _dataReceived;

	public Stream BaseStream
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.BaseStream_Invalid_Not_Open);
			}
			return _internalSerialStream;
		}
	}

	public int BaudRate
	{
		get
		{
			return _baudRate;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("BaudRate", System.SR.ArgumentOutOfRange_NeedPosNum);
			}
			if (IsOpen)
			{
				_internalSerialStream.BaudRate = value;
			}
			_baudRate = value;
		}
	}

	public bool BreakState
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			return _internalSerialStream.BreakState;
		}
		set
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			_internalSerialStream.BreakState = value;
		}
	}

	public int BytesToWrite
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			return _internalSerialStream.BytesToWrite;
		}
	}

	public int BytesToRead
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			return _internalSerialStream.BytesToRead + CachedBytesToRead;
		}
	}

	private int CachedBytesToRead => _readLen - _readPos;

	public bool CDHolding
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			return _internalSerialStream.CDHolding;
		}
	}

	public bool CtsHolding
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			return _internalSerialStream.CtsHolding;
		}
	}

	public int DataBits
	{
		get
		{
			return _dataBits;
		}
		set
		{
			if (value < 5 || value > 8)
			{
				throw new ArgumentOutOfRangeException("DataBits", System.SR.Format(System.SR.ArgumentOutOfRange_Bounds_Lower_Upper, 5, 8));
			}
			if (IsOpen)
			{
				_internalSerialStream.DataBits = value;
			}
			_dataBits = value;
		}
	}

	public bool DiscardNull
	{
		get
		{
			return _discardNull;
		}
		set
		{
			if (IsOpen)
			{
				_internalSerialStream.DiscardNull = value;
			}
			_discardNull = value;
		}
	}

	public bool DsrHolding
	{
		get
		{
			if (!IsOpen)
			{
				throw new InvalidOperationException(System.SR.Port_not_open);
			}
			return _internalSerialStream.DsrHolding;
		}
	}

	public bool DtrEnable
	{
		get
		{
			if (IsOpen)
			{
				_dtrEnable = _internalSerialStream.DtrEnable;
			}
			return _dtrEnable;
		}
		set
		{
			if (IsOpen)
			{
				_internalSerialStream.DtrEnable = value;
			}
			_dtrEnable = value;
		}
	}

	public Encoding Encoding
	{
		get
		{
			return _encoding;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Encoding");
			}
			if (!(value is ASCIIEncoding) && !(value is UTF8Encoding) && !(value is UnicodeEncoding) && !(value is UTF32Encoding) && value.CodePage >= 50000 && value.CodePage != 54936)
			{
				throw new ArgumentException(System.SR.Format(System.SR.NotSupportedEncoding, value.WebName), "Encoding");
			}
			_encoding = value;
			_decoder = _encoding.GetDecoder();
			_maxByteCountForSingleChar = _encoding.GetMaxByteCount(1);
			_singleCharBuffer = null;
		}
	}

	public Handshake Handshake
	{
		get
		{
			return _handshake;
		}
		set
		{
			if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
			{
				throw new ArgumentOutOfRangeException("Handshake", System.SR.ArgumentOutOfRange_Enum);
			}
			if (IsOpen)
			{
				_internalSerialStream.Handshake = value;
			}
			_handshake = value;
		}
	}

	public bool IsOpen
	{
		get
		{
			if (_internalSerialStream != null)
			{
				return _internalSerialStream.IsOpen;
			}
			return false;
		}
	}

	public string NewLine
	{
		get
		{
			return _newLine;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("NewLine");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(System.SR.Format(System.SR.InvalidNullEmptyArgument, "NewLine"), "NewLine");
			}
			_newLine = value;
		}
	}

	public Parity Parity
	{
		get
		{
			return _parity;
		}
		set
		{
			if (value < Parity.None || value > Parity.Space)
			{
				throw new ArgumentOutOfRangeException("Parity", System.SR.ArgumentOutOfRange_Enum);
			}
			if (IsOpen)
			{
				_internalSerialStream.Parity = value;
			}
			_parity = value;
		}
	}

	public byte ParityReplace
	{
		get
		{
			return _parityReplace;
		}
		set
		{
			if (IsOpen)
			{
				_internalSerialStream.ParityReplace = value;
			}
			_parityReplace = value;
		}
	}

	public string PortName
	{
		get
		{
			return _portName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PortName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(System.SR.PortNameEmpty_String, "PortName");
			}
			if (IsOpen)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.Cant_be_set_when_open, "PortName"));
			}
			_portName = value;
		}
	}

	public int ReadBufferSize
	{
		get
		{
			return _readBufferSize;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("ReadBufferSize");
			}
			if (IsOpen)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.Cant_be_set_when_open, "ReadBufferSize"));
			}
			_readBufferSize = value;
		}
	}

	public int ReadTimeout
	{
		get
		{
			return _readTimeout;
		}
		set
		{
			if (value < 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("ReadTimeout", System.SR.ArgumentOutOfRange_Timeout);
			}
			if (IsOpen)
			{
				_internalSerialStream.ReadTimeout = value;
			}
			_readTimeout = value;
		}
	}

	public int ReceivedBytesThreshold
	{
		get
		{
			return _receivedBytesThreshold;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("ReceivedBytesThreshold", System.SR.ArgumentOutOfRange_NeedPosNum);
			}
			_receivedBytesThreshold = value;
			if (IsOpen)
			{
				SerialDataReceivedEventArgs e = new SerialDataReceivedEventArgs(SerialData.Chars);
				CatchReceivedEvents(this, e);
			}
		}
	}

	public bool RtsEnable
	{
		get
		{
			if (IsOpen)
			{
				_rtsEnable = _internalSerialStream.RtsEnable;
			}
			return _rtsEnable;
		}
		set
		{
			if (IsOpen)
			{
				_internalSerialStream.RtsEnable = value;
			}
			_rtsEnable = value;
		}
	}

	public StopBits StopBits
	{
		get
		{
			return _stopBits;
		}
		set
		{
			if (value < StopBits.One || value > StopBits.OnePointFive)
			{
				throw new ArgumentOutOfRangeException("StopBits", System.SR.ArgumentOutOfRange_Enum);
			}
			if (IsOpen)
			{
				_internalSerialStream.StopBits = value;
			}
			_stopBits = value;
		}
	}

	public int WriteBufferSize
	{
		get
		{
			return _writeBufferSize;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("WriteBufferSize");
			}
			if (IsOpen)
			{
				throw new InvalidOperationException(System.SR.Format(System.SR.Cant_be_set_when_open, "WriteBufferSize"));
			}
			_writeBufferSize = value;
		}
	}

	public int WriteTimeout
	{
		get
		{
			return _writeTimeout;
		}
		set
		{
			if (value <= 0 && value != -1)
			{
				throw new ArgumentOutOfRangeException("WriteTimeout", System.SR.ArgumentOutOfRange_WriteTimeout);
			}
			if (IsOpen)
			{
				_internalSerialStream.WriteTimeout = value;
			}
			_writeTimeout = value;
		}
	}

	public event SerialErrorReceivedEventHandler ErrorReceived;

	public event SerialPinChangedEventHandler PinChanged;

	public event SerialDataReceivedEventHandler DataReceived
	{
		add
		{
			bool flag = _dataReceived == null;
			_dataReceived = (SerialDataReceivedEventHandler)Delegate.Combine(_dataReceived, value);
			if (flag && _internalSerialStream != null)
			{
				_internalSerialStream.DataReceived += _dataReceivedHandler;
			}
		}
		remove
		{
			_dataReceived = (SerialDataReceivedEventHandler)Delegate.Remove(_dataReceived, value);
			if (_dataReceived == null && _internalSerialStream != null)
			{
				_internalSerialStream.DataReceived -= _dataReceivedHandler;
			}
		}
	}

	public SerialPort()
	{
		_dataReceivedHandler = CatchReceivedEvents;
	}

	public SerialPort(IContainer container)
		: this()
	{
		container.Add(this);
	}

	public SerialPort(string portName)
		: this(portName, 9600, Parity.None, 8, StopBits.One)
	{
	}

	public SerialPort(string portName, int baudRate)
		: this(portName, baudRate, Parity.None, 8, StopBits.One)
	{
	}

	public SerialPort(string portName, int baudRate, Parity parity)
		: this(portName, baudRate, parity, 8, StopBits.One)
	{
	}

	public SerialPort(string portName, int baudRate, Parity parity, int dataBits)
		: this(portName, baudRate, parity, dataBits, StopBits.One)
	{
	}

	public SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
		: this()
	{
		PortName = portName;
		BaudRate = baudRate;
		Parity = parity;
		DataBits = dataBits;
		StopBits = stopBits;
	}

	public void Close()
	{
		Dispose();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && IsOpen)
		{
			_internalSerialStream.DataReceived -= _dataReceivedHandler;
			_internalSerialStream.Flush();
			_internalSerialStream.Close();
			_internalSerialStream = null;
		}
		base.Dispose(disposing);
	}

	public void DiscardInBuffer()
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		_internalSerialStream.DiscardInBuffer();
		_readPos = (_readLen = 0);
	}

	public void DiscardOutBuffer()
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		_internalSerialStream.DiscardOutBuffer();
	}

	public void Open()
	{
		if (IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_already_open);
		}
		_internalSerialStream = new SerialStream(_portName, _baudRate, _parity, _dataBits, _stopBits, _readTimeout, _writeTimeout, _handshake, _dtrEnable, _rtsEnable, _discardNull, _parityReplace);
		_internalSerialStream.SetBufferSizes(_readBufferSize, _writeBufferSize);
		_internalSerialStream.ErrorReceived += CatchErrorEvents;
		_internalSerialStream.PinChanged += CatchPinChangedEvents;
		if (_dataReceived != null)
		{
			_internalSerialStream.DataReceived += _dataReceivedHandler;
		}
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(System.SR.Argument_InvalidOffLen);
		}
		int num = 0;
		if (CachedBytesToRead >= 1)
		{
			num = Math.Min(CachedBytesToRead, count);
			Buffer.BlockCopy(_inBuffer, _readPos, buffer, offset, num);
			_readPos += num;
			if (num == count)
			{
				if (_readPos == _readLen)
				{
					_readPos = (_readLen = 0);
				}
				return count;
			}
			if (BytesToRead == 0)
			{
				return num;
			}
		}
		_readLen = (_readPos = 0);
		int count2 = count - num;
		num += _internalSerialStream.Read(buffer, offset + num, count2);
		_decoder.Reset();
		return num;
	}

	public int ReadChar()
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		return ReadOneChar(_readTimeout);
	}

	private int ReadOneChar(int timeout)
	{
		int num = 0;
		if (_decoder.GetCharCount(_inBuffer, _readPos, CachedBytesToRead) != 0)
		{
			int readPos = _readPos;
			do
			{
				_readPos++;
			}
			while (_decoder.GetCharCount(_inBuffer, readPos, _readPos - readPos) < 1);
			try
			{
				_decoder.GetChars(_inBuffer, readPos, _readPos - readPos, _oneChar, 0);
			}
			catch
			{
				_readPos = readPos;
				throw;
			}
			return _oneChar[0];
		}
		if (timeout == 0)
		{
			int num2 = _internalSerialStream.BytesToRead;
			if (num2 == 0)
			{
				num2 = 1;
			}
			MaybeResizeBuffer(num2);
			_readLen += _internalSerialStream.Read(_inBuffer, _readLen, num2);
			if (ReadBufferIntoChars(_oneChar, 0, 1, countMultiByteCharsAsOne: false) == 0)
			{
				throw new TimeoutException();
			}
			return _oneChar[0];
		}
		int tickCount = Environment.TickCount;
		do
		{
			int num3;
			if (timeout == -1)
			{
				num3 = _internalSerialStream.ReadByte(-1);
			}
			else
			{
				if (timeout - num < 0)
				{
					throw new TimeoutException();
				}
				num3 = _internalSerialStream.ReadByte(timeout - num);
				num = Environment.TickCount - tickCount;
			}
			MaybeResizeBuffer(1);
			_inBuffer[_readLen++] = (byte)num3;
		}
		while (_decoder.GetCharCount(_inBuffer, _readPos, _readLen - _readPos) < 1);
		_decoder.GetChars(_inBuffer, _readPos, _readLen - _readPos, _oneChar, 0);
		_readLen = (_readPos = 0);
		return _oneChar[0];
	}

	public int Read(char[] buffer, int offset, int count)
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(System.SR.Argument_InvalidOffLen);
		}
		return InternalRead(buffer, offset, count, _readTimeout, countMultiByteCharsAsOne: false);
	}

	private int InternalRead(char[] buffer, int offset, int count, int timeout, bool countMultiByteCharsAsOne)
	{
		if (count == 0)
		{
			return 0;
		}
		int tickCount = Environment.TickCount;
		int bytesToRead = _internalSerialStream.BytesToRead;
		MaybeResizeBuffer(bytesToRead);
		_readLen += _internalSerialStream.Read(_inBuffer, _readLen, bytesToRead);
		int charCount = _decoder.GetCharCount(_inBuffer, _readPos, CachedBytesToRead);
		if (charCount > 0)
		{
			return ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
		}
		if (timeout == 0)
		{
			throw new TimeoutException();
		}
		int maxByteCount = Encoding.GetMaxByteCount(count);
		do
		{
			MaybeResizeBuffer(maxByteCount);
			_readLen += _internalSerialStream.Read(_inBuffer, _readLen, maxByteCount);
			int num = ReadBufferIntoChars(buffer, offset, count, countMultiByteCharsAsOne);
			if (num > 0)
			{
				return num;
			}
		}
		while (timeout == -1 || timeout - GetElapsedTime(Environment.TickCount, tickCount) > 0);
		throw new TimeoutException();
	}

	private int ReadBufferIntoChars(char[] buffer, int offset, int count, bool countMultiByteCharsAsOne)
	{
		int num = Math.Min(count, CachedBytesToRead);
		DecoderReplacementFallback decoderReplacementFallback = _encoding.DecoderFallback as DecoderReplacementFallback;
		if (_encoding.IsSingleByte && _encoding.GetMaxCharCount(num) == num && decoderReplacementFallback != null && decoderReplacementFallback.MaxCharCount == 1)
		{
			_decoder.GetChars(_inBuffer, _readPos, num, buffer, offset);
			_readPos += num;
			if (_readPos == _readLen)
			{
				_readPos = (_readLen = 0);
			}
			return num;
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = _readPos;
		do
		{
			int num5 = Math.Min(count - num3, _readLen - _readPos - num2);
			if (num5 <= 0)
			{
				break;
			}
			num2 += num5;
			num5 = _readPos + num2 - num4;
			int charCount = _decoder.GetCharCount(_inBuffer, num4, num5);
			if (charCount > 0)
			{
				if (num3 + charCount > count && !countMultiByteCharsAsOne)
				{
					break;
				}
				int num6 = num5;
				do
				{
					num6--;
				}
				while (_decoder.GetCharCount(_inBuffer, num4, num6) == charCount);
				_decoder.GetChars(_inBuffer, num4, num6 + 1, buffer, offset + num3);
				num4 = num4 + num6 + 1;
			}
			num3 += charCount;
		}
		while (num3 < count && num2 < CachedBytesToRead);
		_readPos = num4;
		if (_readPos == _readLen)
		{
			_readPos = (_readLen = 0);
		}
		return num3;
	}

	public int ReadByte()
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (_readLen != _readPos)
		{
			return _inBuffer[_readPos++];
		}
		_decoder.Reset();
		return _internalSerialStream.ReadByte();
	}

	public string ReadExisting()
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		byte[] array = new byte[BytesToRead];
		if (_readPos < _readLen)
		{
			Buffer.BlockCopy(_inBuffer, _readPos, array, 0, CachedBytesToRead);
		}
		_internalSerialStream.Read(array, CachedBytesToRead, array.Length - CachedBytesToRead);
		Decoder decoder = Encoding.GetDecoder();
		int charCount = decoder.GetCharCount(array, 0, array.Length);
		int num = array.Length;
		if (charCount == 0)
		{
			Buffer.BlockCopy(array, 0, _inBuffer, 0, array.Length);
			_readPos = 0;
			_readLen = array.Length;
			return "";
		}
		do
		{
			decoder.Reset();
			num--;
		}
		while (decoder.GetCharCount(array, 0, num) == charCount);
		_readPos = 0;
		_readLen = array.Length - (num + 1);
		Buffer.BlockCopy(array, num + 1, _inBuffer, 0, array.Length - (num + 1));
		return Encoding.GetString(array, 0, num + 1);
	}

	public string ReadLine()
	{
		return ReadTo(NewLine);
	}

	public string ReadTo(string value)
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length == 0)
		{
			throw new ArgumentException(System.SR.Format(System.SR.InvalidNullEmptyArgument, "value"), "value");
		}
		int tickCount = Environment.TickCount;
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		char c = value[value.Length - 1];
		int bytesToRead = _internalSerialStream.BytesToRead;
		MaybeResizeBuffer(bytesToRead);
		_readLen += _internalSerialStream.Read(_inBuffer, _readLen, bytesToRead);
		int readPos = _readPos;
		if (_singleCharBuffer == null)
		{
			_singleCharBuffer = new char[_maxByteCountForSingleChar];
		}
		try
		{
			while (true)
			{
				int num2;
				if (_readTimeout == -1)
				{
					num2 = InternalRead(_singleCharBuffer, 0, 1, _readTimeout, countMultiByteCharsAsOne: true);
				}
				else
				{
					if (_readTimeout - num < 0)
					{
						throw new TimeoutException();
					}
					int tickCount2 = Environment.TickCount;
					num2 = InternalRead(_singleCharBuffer, 0, 1, _readTimeout - num, countMultiByteCharsAsOne: true);
					num += Environment.TickCount - tickCount2;
				}
				stringBuilder.Append(_singleCharBuffer, 0, num2);
				if (c != _singleCharBuffer[num2 - 1] || stringBuilder.Length < value.Length)
				{
					continue;
				}
				bool flag = true;
				for (int i = 2; i <= value.Length; i++)
				{
					if (value[value.Length - i] != stringBuilder[stringBuilder.Length - i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			string result = stringBuilder.ToString(0, stringBuilder.Length - value.Length);
			if (_readPos == _readLen)
			{
				_readPos = (_readLen = 0);
			}
			return result;
		}
		catch
		{
			byte[] bytes = _encoding.GetBytes(stringBuilder.ToString());
			if (bytes.Length != 0)
			{
				int cachedBytesToRead = CachedBytesToRead;
				byte[] array = new byte[cachedBytesToRead];
				if (cachedBytesToRead > 0)
				{
					Buffer.BlockCopy(_inBuffer, _readPos, array, 0, cachedBytesToRead);
				}
				_readPos = 0;
				_readLen = 0;
				MaybeResizeBuffer(bytes.Length + cachedBytesToRead);
				Buffer.BlockCopy(bytes, 0, _inBuffer, _readLen, bytes.Length);
				_readLen += bytes.Length;
				if (cachedBytesToRead > 0)
				{
					Buffer.BlockCopy(array, 0, _inBuffer, _readLen, cachedBytesToRead);
					_readLen += cachedBytesToRead;
				}
			}
			throw;
		}
	}

	public void Write(string text)
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length != 0)
		{
			byte[] bytes = _encoding.GetBytes(text);
			_internalSerialStream.Write(bytes, 0, bytes.Length, _writeTimeout);
		}
	}

	public void Write(char[] buffer, int offset, int count)
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(System.SR.Argument_InvalidOffLen);
		}
		if (buffer.Length != 0)
		{
			byte[] bytes = Encoding.GetBytes(buffer, offset, count);
			Write(bytes, 0, bytes.Length);
		}
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		if (!IsOpen)
		{
			throw new InvalidOperationException(System.SR.Port_not_open);
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", System.SR.ArgumentOutOfRange_NeedNonNegNumRequired);
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException(System.SR.Argument_InvalidOffLen);
		}
		if (buffer.Length != 0)
		{
			_internalSerialStream.Write(buffer, offset, count, _writeTimeout);
		}
	}

	public void WriteLine(string text)
	{
		Write(text + NewLine);
	}

	private void CatchErrorEvents(object src, SerialErrorReceivedEventArgs e)
	{
		SerialErrorReceivedEventHandler errorReceived = this.ErrorReceived;
		SerialStream internalSerialStream = _internalSerialStream;
		if (errorReceived == null || internalSerialStream == null)
		{
			return;
		}
		lock (internalSerialStream)
		{
			if (internalSerialStream.IsOpen)
			{
				errorReceived(this, e);
			}
		}
	}

	private void CatchPinChangedEvents(object src, SerialPinChangedEventArgs e)
	{
		SerialPinChangedEventHandler pinChanged = this.PinChanged;
		SerialStream internalSerialStream = _internalSerialStream;
		if (pinChanged == null || internalSerialStream == null)
		{
			return;
		}
		lock (internalSerialStream)
		{
			if (internalSerialStream.IsOpen)
			{
				pinChanged(this, e);
			}
		}
	}

	private void CatchReceivedEvents(object src, SerialDataReceivedEventArgs e)
	{
		SerialDataReceivedEventHandler dataReceived = _dataReceived;
		SerialStream internalSerialStream = _internalSerialStream;
		if (dataReceived == null || internalSerialStream == null)
		{
			return;
		}
		lock (internalSerialStream)
		{
			bool flag = false;
			try
			{
				flag = internalSerialStream.IsOpen && (SerialData.Eof == e.EventType || BytesToRead >= _receivedBytesThreshold);
			}
			catch
			{
			}
			finally
			{
				if (flag)
				{
					dataReceived(this, e);
				}
			}
		}
	}

	private void CompactBuffer()
	{
		Buffer.BlockCopy(_inBuffer, _readPos, _inBuffer, 0, CachedBytesToRead);
		_readLen = CachedBytesToRead;
		_readPos = 0;
	}

	private void MaybeResizeBuffer(int additionalByteLength)
	{
		if (additionalByteLength + _readLen > _inBuffer.Length)
		{
			if (CachedBytesToRead + additionalByteLength <= _inBuffer.Length / 2)
			{
				CompactBuffer();
				return;
			}
			int num = Math.Max(CachedBytesToRead + additionalByteLength, _inBuffer.Length * 2);
			byte[] array = new byte[num];
			Buffer.BlockCopy(_inBuffer, _readPos, array, 0, CachedBytesToRead);
			_readLen = CachedBytesToRead;
			_readPos = 0;
			_inBuffer = array;
		}
	}

	private static int GetElapsedTime(int currentTickCount, int startTickCount)
	{
		int num = currentTickCount - startTickCount;
		if (num < 0)
		{
			return int.MaxValue;
		}
		return num;
	}

	public static string[] GetPortNames()
	{
		using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM"))
		{
			if (registryKey != null)
			{
				string[] valueNames = registryKey.GetValueNames();
				for (int i = 0; i < valueNames.Length; i++)
				{
					valueNames[i] = (string)registryKey.GetValue(valueNames[i]);
				}
				return valueNames;
			}
		}
		return ArrayEx.Empty<string>();
	}
}
