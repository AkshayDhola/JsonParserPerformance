# *JsonParserPerformance*

*JsonParserPerformance is a high-performance JSON parser for Protobuf messages in .NET, designed for speed and minimal memory allocation.*


## *Getting Started*
```cs
using JsonParserPerformance;

// Parse using the high-performance parser
IMessageProtobuf result = JsonParser.Parse<IMessageProtobuf>(jsonStr);

// Alternatively, use the generated Protobuf parser
IMessageProtobuf result = IMessageProtobuf.Parser.ParseFrom(jsonStr);
```

## *Benchmark*

```md

| Method                           | Mean     | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------|---------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
| GoogleProtobuf_JsonParser        | 80.42 us | 1.52 us  | 1.81 us  | 1.00  | 9.3994 | 0.2441 | 38.75 KB  | 1.00        |
| JsonParserPerformance_JsonParser | 53.34 us | 1.05 us  | 1.54 us  | 0.66  | 4.1504 | —      | 17.78 KB  | 0.46        |
```