using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

class PipeClient
{
    static void Main(string[] args)
    {
        Task.Run(async () =>
        {
            await SendAwait("Monkey Pants", "AWS_Pipe_For_Panther", 1000);
        }).Wait();

    }

    public static async Task SendAwait(string SendStr, string PipeName, int TimeOut = 1000)
    {
        using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous))
        {
            // The connect function will indefinitely wait for the pipe to become available
            // If that is not acceptable specify a maximum waiting time (in ms)
            pipeStream.Connect(TimeOut);
           
            using (StreamWriter sw = new StreamWriter(pipeStream))
            {
                await sw.WriteAsync(SendStr);
                // flush
                await pipeStream.FlushAsync();
            }
        }
    }
}