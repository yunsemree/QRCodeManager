using IconGenerator;

if (args.Length != 2)
{
    Console.Error.WriteLine("Kullanim: dotnet run --project tools/IconGenerator -- <pngPath> <icoPath>");
    return 1;
}

IconFileGenerator.CreateFromPng(args[0], args[1]);
Console.WriteLine($"ICO olusturuldu: {args[1]}");
return 0;
