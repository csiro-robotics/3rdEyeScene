
file_header = '''// This is a generated file. Do not modify it directly.
'''

source_types = [
    ('sbyte', 'SByte'),
    ('byte', 'Byte'),
    ('short', 'Int16'),
    ('ushort', 'UInt16'),
    ('int', 'Int32'),
    ('uint', 'UInt32'),
    ('long', 'Int64'),
    ('ulong', 'UInt64'),
    ('float', 'Single'),
    ('double', 'Double'),
]

get_func_template = '''

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>{0}</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange{1}(IList<{0}> dst, IList src, int srcOffset, int count)
    {{
      IList<{2}> srcList = (IList<{2}>)src;
      for (int i = 0; i < count; ++i)
      {{
        dst.Add(({0})srcList[i + srcOffset]);
      }}
    }}

    /// <summary>
    /// Read a single <c>{0}</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public {0} Get{1}(IList src, int srcOffset)
    {{
      IList<{2}> srcList = (IList<{2}>)src;
      return ({0})srcList[srcOffset];
    }}'''

getters_template = ''
for to_build_in, to_system_type in source_types:
    getters_template += get_func_template.format(to_build_in, to_system_type, '@0@')

file_template = '''using System;
using System.Collections;
using System.Collections.Generic;

namespace Tes.Buffers.Converters
{{
  /// <summary>
  /// Type conversion helper from <c>{0}</c> typed List and Array types.
  /// </summary>
  internal class {1}Converter : BufferConverter
  {{
    /// <summary>
    /// Query the supported buffer type.
    /// </summary>
    public Type Type {{ get {{ return typeof({0}); }} }}

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    public int AddressableCount(IList list)
    {{
      return ((IList<{0}>)list).Count;
    }}{2}
  }}
}}
'''

for from_built_in, from_system_type in source_types:
    # Build a string from the get template for each target type from the current type
    template_content = file_template.format('@0@', '@1@', getters_template)
    template_content = template_content.replace('{', '{{')
    template_content = template_content.replace('}', '}}')
    template_content = template_content.replace(r'@0@', '{0}')
    template_content = template_content.replace(r'@1@', '{1}')
    custom_content = template_content.format(from_built_in, from_system_type)
    with open('{}Converter.cs'.format(from_system_type), 'w') as source_file:
        source_file.write(file_header)
        source_file.write(custom_content)

####
vector_converters = [
  ('Vector2', 2),
  ('Vector3', 3),
]

vector_get_func_template = '''

    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>{0}</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    public void GetRange{1}(IList<{0}> dst, IList src, int srcOffset, int count)
    {{
      IList<{2}> srcList = (IList<{2}>)src;
      int initialComponent = srcOffset % {3};
      for (int i = 0; i < count / {3}; ++i)
      {{
        for (int j = initialComponent; j < {3} && j + i * {3} < count; ++j)
        {{
          dst.Add(({0})srcList[i + srcOffset / {3}][j]);
        }}
      }}
    }}

    /// <summary>
    /// Read a single <c>{0}</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    public {0} Get{1}(IList src, int srcOffset)
    {{
      IList<{2}> srcList = (IList<{2}>)src;
      return ({0})srcList[srcOffset / {3}][srcOffset % {3}];
    }}'''

vector_getters_template = ''
for to_build_in, to_system_type in source_types:
    vector_getters_template += vector_get_func_template.format(to_build_in, to_system_type, '@0@', '@1@')

vector_converter_template = '''using System;
using System.Collections;
using System.Collections.Generic;
using Tes.Maths;

namespace Tes.Buffers.Converters
{{
  /// <summary>
  /// Type conversion helper from {0} typed List and Array types.
  /// </summary>
  internal class {0}Converter : BufferConverter
  {{
    /// <summary>
    /// Query the supported buffer type.
    /// </summary>
    public Type Type {{ get {{ return typeof({0}); }} }}

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    public int AddressableCount(IList list)
    {{
      return ((IList<{0}>)list).Count * {1};
    }}{2}
  }}
}}
'''

for vector_type, component_count in vector_converters:
    template_content = vector_converter_template.format('@0@', '@1@', vector_getters_template)
    template_content = template_content.replace('{', '{{')
    template_content = template_content.replace('}', '}}')
    template_content = template_content.replace(r'@0@', '{0}')
    template_content = template_content.replace(r'@1@', '{1}')
    custom_content = template_content.format(vector_type, component_count)
    with open('{}Converter.cs'.format(vector_type), 'w') as source_file:
        source_file.write(file_header)
        source_file.write(custom_content)

###
get_func_proto_template = '''
    /// <summary>
    /// Extract <paramref name="count"/> values of type <c>{0}</c> from <paramref name="src"/>.
    /// </summary>
    /// <param name="dst">The list to add values to using <c>IList.Add()</c>.</param>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to start reading at.</param>
    /// <param name="count">The number of items to read into <paramref name="dst"/>.</param>
    void GetRange{1}(IList<{0}> dst, IList src, int srcOffset, int count);
    /// <summary>
    /// Read a single <c>{0}</c> value from <paramref name="src"/>.
    /// </summary>
    /// <param name="src">The source container to read from.</param>
    /// <param name="srcOffset">The index offset into <paramref name="source"/> to read from.</param>
    /// <return>The value at the requested offset.</return>
    {0} Get{1}(IList src, int srcOffset);'''

interface_template = '''using System;
using System.Collections;
using System.Collections.Generic;

namespace Tes.Buffers.Converters
{{
  /// <summary>
  /// Interface for converting from <c>IList</c> wrapped in a <c><see cref="VertexBuffer"/></c>.
  /// </summary>
  /// <remarks>
  /// The interface consists of a series of <c>Get&lt;Type&gt;()</c> and <c>GetRange&lt;Type&gt;()</c> functions.
  /// As this the converter operates with the internals of a <c><see cref="VertexBuffer"/></c>, the supported source
  /// list arguments will generally be simple types, or of known types with data channels. See
  /// <see cref="VertexBuffer"/> for details on how index and count arguments are treated.
  /// </remarks>
  internal interface BufferConverter
  {{
    /// <summary>
    /// Query the supported buffer type. This is the type contained in the <c>IList</c>.
    /// </summary>
    Type Type {{ get; }}

    /// <summary>
    /// Query the number of addressable elements in the array. This includes addressing individual data components.
    /// </summary>
    int AddressableCount(IList list);
{0}
  }}
}}
'''

with open('BufferConverter.cs', 'w') as source_file:
    getters_proto = ''
    for to_build_in, to_system_type in source_types:
        getters_proto += get_func_proto_template.format(to_build_in, to_system_type)
    interface_content = interface_template.format(getters_proto)
    source_file.write(file_header)
    source_file.write(interface_content)


####
add_converter_template = '''      _converters.Add(typeof({0}), new {1}Converter());
'''

converter_set_template = '''using System;
using System.Collections.Generic;
using Tes.Maths;

namespace Tes.Buffers.Converters
{{
  /// <summary>
  /// Defines a collection of <see cref="BufferConverter"/> bound to explicit buffer types.
  /// </summary>
  /// <remarks>
  /// A <see cref="BufferConverter"/> may be retrieved for a specific <c>Type</c>. Supported types are all built in
  /// types except for <c>bool</c> and Tes vector types :
  /// [{1}]
  /// </remarks>
  internal static class ConverterSet
  {{
    /// <summary>
    /// Get the <see cref="BufferConverter"/> for <paramref name="forType"/>
    /// </summary>
    /// <remarks>
    /// <paramref name="forType"/> must be a supported type or an exception will be thrown.
    /// </remarks>
    /// <param name="forType">The type to get a converter for.</param>
    /// <return>The converter for the requested type.</return>
    internal static BufferConverter Get(Type forType)
    {{
      return _converters[forType];
    }}

    static ConverterSet()
    {{
{0}    }}

    private static Dictionary<Type, BufferConverter> _converters = new Dictionary<Type, BufferConverter>();
  }}
}}
'''

with open('ConverterSet.cs', 'w') as source_file:
    add_converters = ''
    type_list = ''
    type_list_append = ''
    for from_built_in, from_system_type in source_types:
        add_converters += add_converter_template.format(from_built_in, from_system_type)
        type_list += type_list_append + from_built_in
        type_list_append = ', '
    for vector_type, _ in vector_converters:
        add_converters += add_converter_template.format(vector_type, vector_type)
        type_list += type_list_append + from_built_in
        type_list_append = ', '
    converter_set_content = converter_set_template.format(add_converters, type_list)
    source_file.write(file_header)
    source_file.write(converter_set_content)
