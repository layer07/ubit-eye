namespace MinerPulse
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Init.Chk();
			while (true)
			{
				Thread.Sleep(1000);
			}
		}
	}
}
