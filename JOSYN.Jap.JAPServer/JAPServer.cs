using JOSYN.Foundation.PropertyBag;
using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Shared.Contract;
using JOSYN.Jap.Shared.Log;

namespace JOSYN.Jap.JAPServer;

/// <summary>
/// This is a fake-implementation by now...
/// </summary>
internal sealed class JAPServer : IJosynApplicationProtocol
{
    async Task<Result<string>> IJosynApplicationProtocol.GetRawArguments()
    {
        return await Task.FromResult(FakeReadArgumentsFromFile());
    }

    async Task<Result> IJosynApplicationProtocol.PutRawResult(string result)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[PROCESSING]");
        Console.WriteLine(result);
        Console.WriteLine();
        Console.ResetColor();

        return await Task.FromResult(Result.Success);
    }

    async Task<Result> IJosynApplicationProtocol.PutError(string serializedError)
    {
        var deserialize = PropertyBag.Deserialize<ErrorReport>(serializedError);
        if (!deserialize.Succeeded)
        {
            LocalLog.WriteError($"ErrorReport konnte nicht deserialisiert werden: {deserialize.ErrorMessage}\nRaw: {serializedError}");
            return Result.Propagate(deserialize.ToResult());
        }
        var report = deserialize.Value;
        LocalLog.WriteError(report.Causer, report.Message, report.CallStack, report.ExceptionDetails);
        return await Task.FromResult(Result.Success);
    }

    internal static string FakeReadArgumentsFromFile()
    {
        const string inicontent = """
                                  Msg=Hello JOSYN
                                  Count=9
                                  MaybeCount=
                                  IsSpecial=True
                                  Expired=21.09.1988 00:00:00
                                  OnlyDate=04.11.1966
                                  MaybeDate=
                                  EnumValue=Value2
                                  MyTimeSpan=09:10:59
                                  Price=1.200,30
                                  """;
        return inicontent;
    }
}
