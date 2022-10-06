# Measurement generator for KensoBI and QDAS schemas


First, update database connection strings with user's password in `\MeasurementGenerator\appsettings.json`.

Next, compile and run it:

```console
dotnet build
cd MeasurementGenerator\bin\Debug\net6.0
.\MeasurementGenerator --schema 1
```
Execute the app with `--help` argument so see all options. 