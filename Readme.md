# SimpleWebServer

So simple web server library that a cat can use

![really](/cat.png)

## How to use

```csharp
[Mapping("hello")]
public static string CounterCallback()
{
    return "<body><h3>Hello, World!</h3></body>";
}
```