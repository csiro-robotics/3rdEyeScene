using System;
using System.Collections.Generic;
using System.Reflection;
using Tes;
using Tes.Runtime;

namespace Tes.Main
{
  public class PluginManager
  {
    public PluginManager()
    {
    }

    public void LoadFrom<T>(List<T> instances, Type targetType, string assemblyPath, object[] args) where T : class
    {
      Assembly potentialPlugin = Assembly.LoadFile(assemblyPath);
      bool addedAsPlugin = false;
      if (potentialPlugin != null)
      {
        Type[] argsTypes = new Type[args.Length];
        for (int i = 0; i< argsTypes.Length; ++i)
        {
          argsTypes[i] = args[i].GetType();
        }
        foreach (Type type in potentialPlugin.GetExportedTypes())
        {
          if (type.IsSubclassOf(targetType) && !type.IsAbstract)
          {
            // Found a candidate.
            T instance = null;
            ConstructorInfo constructor = type.GetConstructor(argsTypes);

            if (constructor != null)
            {
              instance = constructor.Invoke(args) as T;
            }
            else
            {
              constructor = type.GetConstructor(null);
              if (constructor != null)
              {
                instance = constructor.Invoke(null) as T;
              }
            }

            if (instance != null)
            {
              // Have a valid instance. Register the assembly.
              instances.Add(instance);
              if (!addedAsPlugin && !_plugins.Contains(potentialPlugin))
              {
                _plugins.Add(potentialPlugin);
              }
              addedAsPlugin = true;
            }
          }
        }
      }
    }

    private List<Assembly> _plugins = new List<Assembly>();
  }
}

