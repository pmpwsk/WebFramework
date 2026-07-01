using System.Reflection;
using System.Runtime.ExceptionServices;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework.Plugins;

/// <summary>
/// Includes helper methods to discover endpoints in a plugin using reflection.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// Builds a dictionary of paths and their endpoint handlers using reflection. 
    /// </summary>
    public static Dictionary<string, Func<Request, Task<IResponse>>> BuildEndpoints(IPlugin plugin)
    {
        Dictionary<string, Func<Request, Task<IResponse>>> endpoints = [];
        var type = plugin.GetType();
        
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static))
        {
            var attribute = method.GetCustomAttribute<EndpointAttribute>();
            if (attribute == null)
                continue;
            var path = attribute.Path;
            if (endpoints.ContainsKey(path))
            {
                Console.WriteLine($"Endpoint '{path}' is defined in multiple methods.");
                continue;
            }
            
            Func<Request, object[]> parameterFunction;
            if (EndpointAcceptsNothing(method))
            {
                parameterFunction = _ => [];
            }
            else if (EndpointAcceptsRequest(method))
            {
                parameterFunction = req => [req];
            }
            else
            {
                Console.WriteLine($"Endpoint '{path}' in {plugin.GetType().Name} has an incorrect parameter list.");
                continue;
            }
            
            Func<object?, Task<IResponse>> resultFunction;
            if (EndpointReturnsResponse(method))
            {
                resultFunction = result =>
                    result is IResponse response
                        ? Task.FromResult(response)
                        : throw new Exception();
            }
            else if (EndpointReturnsResponseTask(method))
            {
                resultFunction = async result =>
                    result is Task task && await (dynamic)task is IResponse response
                        ? response
                        : throw new Exception("Invalid endpoint return value.");
            }
            else
            {
                Console.WriteLine($"Endpoint '{path}' in {plugin.GetType().Name} has an incorrect return type.");
                continue;
            }
            
            endpoints[path] = req =>
            {
                try
                {
                    return resultFunction(method.Invoke(plugin, parameterFunction(req)));
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    throw;
                }
            };
        }
        
        return endpoints;
    }
    
    /// <summary>
    /// Returns whether the method's parameter list is empty.
    /// </summary>
    private static bool EndpointAcceptsNothing(MethodInfo method)
        => method.GetParameters().Length == 0;
    
    /// <summary>
    /// Returns whether the method's parameter list is a single parameter of the type <c>Request</c>.
    /// </summary>
    private static bool EndpointAcceptsRequest(MethodInfo method)
        => method.GetParameters().Length == 1
           && method.GetParameters()[0].ParameterType == typeof(Request);
    
    /// <summary>
    /// Returns whether the method returns an <c>IResponse</c> object.
    /// </summary>
    private static bool EndpointReturnsResponse(MethodInfo method)
        => typeof(IResponse).IsAssignableFrom(method.ReturnType);
    
    /// <summary>
    /// Returns whether the method returns a task which results in an <c>IResponse</c> object.
    /// </summary>
    private static bool EndpointReturnsResponseTask(MethodInfo method)
        => method.ReturnType.IsGenericType
           && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
           && method.ReturnType.GetGenericArguments().Length == 1
           && typeof(IResponse).IsAssignableFrom(method.ReturnType.GetGenericArguments()[0]);
}