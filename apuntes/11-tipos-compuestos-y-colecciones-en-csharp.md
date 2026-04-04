# Tipos compuestos y colecciones en C#

## Introducción

Hasta ahora trabajamos con tipos **primitivos**: `int`, `double`, `bool`. Cada variable guarda un solo valor.

Cuando necesitamos representar información más compleja —una persona, una lista de productos, una tabla de configuración— usamos **tipos compuestos**: estructuras que agrupan o coleccionan múltiples valores bajo una sola variable.

En esta clase veremos siete tipos fundamentales:

| Tipo | ¿Para qué? | Tamaño | Mutabilidad |
|------|-----------|--------|-------------|
| `string` | Texto inmutable | Dinámico | Inmutable |
| `(T1, T2, ...)` Tupla | Agrupar valores temporalmente | Fijo | Mutable |
| `T[]` Array | Secuencia de tamaño fijo | Fijo | Mutable |
| `List<T>` | Secuencia de tamaño dinámico | Dinámico | Mutable |
| `Dictionary<K,V>` | Mapa clave → valor | Dinámico | Mutable |
| `record` | Dato con identidad semántica | Fijo | Inmutable por convención |
| Tipo anónimo | Proyección temporal de datos | Fijo | Inmutable |

---

## 1. String

Aunque `string` parezca un tipo primitivo, en .NET es una **clase** (`System.String`): una secuencia inmutable de caracteres Unicode. Comprender su comportamiento es fundamental para escribir código correcto y eficiente.

### 1.1 Literales y formas de creación

```csharp
// Literal simple
string saludo = "Hola, mundo";

// Verbatim — respeta saltos de línea y barras invertidas literalmente
string ruta    = @"C:\Users\Alejandro\Documents";
string multiln = @"Primera línea
Segunda línea
Tercera línea";

// Raw string literal (C# 11+) — sin necesidad de escapar nada
string json = """
    {
        "nombre": "Ana",
        "edad": 25
    }
    """;

// Interpolado — embebe expresiones con $
string nombre = "Ana";
int    edad   = 25;
string msg    = $"Hola, {nombre}. Tenés {edad} años.";

// Interpolado + verbatim combinados
string query = $@"SELECT *
FROM usuarios
WHERE nombre = '{nombre}'";

// Interpolado + raw string (C# 11+) — el más potente
string html = $"""
    <h1>{nombre}</h1>
    <p>Edad: {edad}</p>
    """;
```

### 1.2 Inmutabilidad — concepto clave

Cada operación sobre un string **crea un objeto nuevo**. El original nunca se modifica:

```csharp
string a = "hola";
string b = a.ToUpper();   // crea un nuevo string "HOLA"

Console.WriteLine(a);     // hola — sin cambios
Console.WriteLine(b);     // HOLA

// Concatenación con + también crea nuevos objetos en cada paso
string resultado = "uno" + " " + "dos";   // genera objetos intermedios
```

> **Implicación de rendimiento:** concatenar strings en un loop con `+` es costoso porque genera un nuevo objeto en cada iteración. Para eso existe `StringBuilder`.

### 1.3 Operaciones comunes

```csharp
string texto = "  Hola, Tucumán!  ";

// Inspección
int    largo    = texto.Length;                      // 18
bool   esvacio  = string.IsNullOrWhiteSpace(texto);  // false
bool   contiene = texto.Contains("Tucumán");          // true
bool   empieza  = texto.StartsWith("  Hola");         // true
int    posicion = texto.IndexOf("Tucumán");            // 7

// Transformación — cada uno retorna un nuevo string
string limpio    = texto.Trim();                        // "Hola, Tucumán!"
string mayus     = texto.ToUpper();                     // "  HOLA, TUCUMÁN!  "
string reemplazo = texto.Replace("Tucumán", "Córdoba");
string sub       = texto.Substring(7, 7);              // "Tucumán"
string sub2      = texto[7..14];                       // "Tucumán" — con rango

// División y unión
string[]  partes = "a,b,c,d".Split(',');               // ["a","b","c","d"]
string    unido  = string.Join(" - ", partes);         // "a - b - c - d"
string    unido2 = string.Join(", ", [1, 2, 3]);       // "1, 2, 3"
```

### 1.4 Comparación

```csharp
string s1 = "hola";
string s2 = "HOLA";

// Igualdad exacta (sensible a mayúsculas)
Console.WriteLine(s1 == s2);   // false

// Ignorando mayúsculas/minúsculas
Console.WriteLine(string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase)); // true

// Comparación para ordenamiento
int ord = string.Compare("abc", "abd", StringComparison.Ordinal);  // -1

// Verificar nulo o vacío
string? valor = null;
Console.WriteLine(string.IsNullOrEmpty(valor));     // true
Console.WriteLine(string.IsNullOrWhiteSpace("  ")); // true
```

### 1.5 StringBuilder — concatenación eficiente

Cuando se construye un string de forma incremental (en loops o con muchas concatenaciones), usar `StringBuilder` es fundamentalmente más eficiente:

```csharp
using System.Text;

// MAL — genera N objetos en memoria en cada iteración
string resultado = "";
for (int i = 0; i < 1000; i++)
    resultado += i.ToString();   // 1000 allocations

// BIEN — trabaja sobre un buffer interno mutable
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
    sb.Append(i);

string final = sb.ToString();   // una sola allocación al final
```

Métodos útiles de `StringBuilder`:

```csharp
var sb = new StringBuilder();

sb.Append("Hola");
sb.AppendLine(", mundo");           // agrega + salto de línea
sb.Insert(0, ">>> ");               // inserta en posición
sb.Replace("mundo", "Tucumán");    // reemplaza en el buffer
sb.AppendFormat("Valor: {0:F2}", 3.14159);

Console.WriteLine(sb.ToString());
```

### 1.6 Interpolación avanzada

```csharp
decimal  precio = 12_500.75m;
DateTime fecha  = DateTime.Now;

// Formatos directamente en la interpolación
Console.WriteLine($"Precio: {precio:N2}");           // 12.500,75
Console.WriteLine($"Fecha:  {fecha:dd/MM/yyyy}");    // 01/04/2026
Console.WriteLine($"Hex:    {255:X}");               // FF
Console.WriteLine($"Pad:    {42,10}");               // alineado a derecha en 10 chars

// Expresiones completas dentro de {}
var nombres = new[] { "Ana", "Carlos", "Laura" };
Console.WriteLine($"Hay {nombres.Length} personas: {string.Join(", ", nombres)}");
```

### 1.7 Span\<char\> — manipulación sin allocación (avanzado)

Para escenarios de alto rendimiento donde no se quiere crear objetos intermedios:

```csharp
string texto = "Hola, mundo";

// Slice sin copiar — sólo una "ventana" sobre el buffer original
ReadOnlySpan<char> span  = texto.AsSpan();
ReadOnlySpan<char> hola  = span[0..4];     // "Hola" — cero allocations
ReadOnlySpan<char> mundo = span[^5..];     // "mundo"

Console.WriteLine(hola.ToString());        // Hola
```

---

## 2. Tuplas

Una **tupla** es la forma más simple de agrupar valores de distintos tipos sin crear una clase.

### 2.1 ¿Cuándo usarlas?

- Retorno múltiple de un método.
- Agrupación temporal dentro de un método.
- No tiene sentido crear un tipo dedicado para esos datos.

### 2.2 Creación

```csharp
// Forma básica — acceso por .Item1, .Item2 (evitar)
var t1 = ("Ana", 25);
Console.WriteLine(t1.Item1); // Ana

// Con nombres de campo — recomendado
(string Nombre, int Edad) persona = ("Ana", 25);
Console.WriteLine(persona.Nombre); // Ana

// Inferencia de nombres desde variables (C# 10+)
var nombre = "Ana";
var edad   = 25;
var p = (nombre, edad);   // campos: p.nombre, p.edad
```

> **Nota:** El tipo real es `System.ValueTuple<T1, T2, ...>`. Los nombres de campo son azúcar sintáctica del compilador y no existen en tiempo de ejecución.

### 2.3 Desestructuración

```csharp
var punto = (X: 10.5, Y: 3.2);

// Desestructurar en variables individuales
var (x, y) = punto;
Console.WriteLine(x); // 10.5

// Descartar campos con _
var (_, soloY) = punto;
```

### 2.4 Retorno múltiple de métodos

```csharp
static (double Min, double Max, double Promedio) Estadisticas(int[] datos)
{
    return (datos.Min(), datos.Max(), datos.Average());
}

// Uso — desestructuración directa
var (min, max, prom) = Estadisticas([3, 7, 1, 9, 2]);
Console.WriteLine($"Min: {min}, Max: {max}, Promedio: {prom:F2}");
```

### 2.5 Pattern matching

```csharp
static string Clasificar((int edad, bool esVip) cliente) => cliente switch
{
    (>= 18, true)  => "Adulto VIP",
    (>= 18, false) => "Adulto estándar",
    (< 18, _)      => "Menor de edad"
};

Console.WriteLine(Clasificar((22, true)));  // Adulto VIP
Console.WriteLine(Clasificar((15, true)));  // Menor de edad
```

### 2.6 Cuándo NO usar tuplas

| Situación | Mejor alternativa |
|-----------|-----------------|
| El dato pasa entre varias capas del sistema | `record` |
| Tiene comportamiento propio (métodos) | `class` o `record` |
| Se usa en colecciones públicas | `record` o `class` |

---

## 3. Arrays

Un **array** es una secuencia de elementos del **mismo tipo**, con un tamaño **fijo** definido al crear el array. Los elementos se almacenan de forma contigua en memoria, lo que permite acceso en O(1) por índice.

### 3.1 Creación

```csharp
// Array vacío con tamaño — valores por defecto (0, null, false según el tipo)
int[] numeros = new int[5];

// Inicialización con valores — literal C# 12+ (recomendado)
int[] pares = [2, 4, 6, 8, 10];

// Sintaxis anterior — también válida
var colores = new string[] { "rojo", "verde", "azul" };

// Con inferencia de tipo
var dias = new[] { "lun", "mar", "mié", "jue", "vie" };
```

### 3.2 Acceso por índice

Los índices comienzan en **0**.

```csharp
var frutas = ["manzana", "banana", "cereza", "durazno"];

Console.WriteLine(frutas[0]);    // manzana  — primer elemento
Console.WriteLine(frutas[^1]);   // durazno  — último elemento
Console.WriteLine(frutas[^2]);   // cereza   — penúltimo

// Modificar un elemento
frutas[1] = "pera";
```

### 3.3 Slicing con rangos

```csharp
var numeros = [10, 20, 30, 40, 50, 60];

var primerosTres = numeros[0..3];   // [10, 20, 30]
var ultimosDos   = numeros[^2..];   // [50, 60]
var medio        = numeros[2..^2];  // [30, 40]
```

> La sintaxis `inicio..fin` excluye el índice `fin`. El operador `^n` cuenta desde el final.

### 3.4 Recorrido

```csharp
var lenguajes = ["C#", "Python", "Java", "Go"];

// foreach — cuando no necesitás el índice
foreach (var lang in lenguajes)
    Console.WriteLine(lang);

// for — cuando necesitás el índice
for (int i = 0; i < lenguajes.Length; i++)
    Console.WriteLine($"[{i}] {lenguajes[i]}");
```

### 3.5 Operaciones comunes

```csharp
var notas = [7, 3, 9, 5, 8, 2, 6];

Array.Sort(notas);                         // [2, 3, 5, 6, 7, 8, 9]
Array.Reverse(notas);                      // [9, 8, 7, 6, 5, 3, 2]

int  pos  = Array.IndexOf(notas, 7);       // índice de la primera ocurrencia
bool hay  = notas.Contains(5);             // true/false
var  copia = (int[])notas.Clone();
```

### 3.6 Arrays multidimensionales

```csharp
// Rectangular — todas las filas con el mismo largo
int[,] matriz =
{
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
};
Console.WriteLine(matriz[1, 2]); // 6

// Array de arrays (jagged) — cada fila puede tener distinto largo
int[][] triangulo = new int[3][];
triangulo[0] = [1];
triangulo[1] = [1, 2];
triangulo[2] = [1, 2, 3];
```

---

## 4. List\<T\>

`List<T>` es la colección dinámica más usada de .NET. A diferencia del array, **puede crecer o reducirse** en tiempo de ejecución.

### 4.1 Tipos genéricos — concepto clave

El `<T>` es el **parámetro de tipo**. Al escribir `List<string>` le decimos al compilador que esta lista sólo puede contener `string`. Esto garantiza seguridad de tipos en tiempo de compilación.

```csharp
List<int>    numeros;   // sólo enteros
List<string> nombres;   // sólo strings
List<Punto>  puntos;    // sólo objetos Punto
```

### 4.2 Creación

```csharp
// Vacía
var nombres = new List<string>();

// Con valores — sintaxis de colección C# 12+ (recomendado)
List<string> ciudades = ["Tucumán", "Córdoba", "Rosario", "Mendoza"];

// Desde array existente
int[] arr  = [1, 2, 3, 4];
var  lista = arr.ToList();

// Con capacidad inicial (optimización cuando sabés el tamaño aproximado)
var grande = new List<int>(capacity: 10_000);
```

### 4.3 Operaciones CRUD

```csharp
var items = new List<string> { "Alpha", "Beta", "Gamma" };

// Agregar
items.Add("Delta");
items.Insert(0, "Omega");
items.AddRange(["Epsilon", "Zeta"]);

// Leer
string primero = items[0];
int    cantidad = items.Count;
bool   existe   = items.Contains("Beta");
int    indice   = items.IndexOf("Beta");

// Modificar
items[1] = "BETA";

// Eliminar
items.Remove("Gamma");
items.RemoveAt(0);
items.RemoveAll(x => x.Length > 4);
items.Clear();
```

### 4.4 LINQ — consultas sobre colecciones

`List<T>` implementa `IEnumerable<T>`, lo que habilita todo el ecosistema LINQ:

```csharp
record Producto(string Nombre, decimal Precio, int Stock);

List<Producto> productos =
[
    new("Monitor 27\"",    450_000, 12),
    new("Teclado mecánico", 85_000, 40),
    new("Mouse inalámbrico", 35_000, 0),
    new("Webcam HD",        55_000, 8),
    new("Auriculares",      70_000, 0),
];

// Filtrar + ordenar + proyectar
var enStock = productos
    .Where(p => p.Stock > 0)
    .OrderBy(p => p.Precio)
    .Select(p => $"{p.Nombre}: ${p.Precio:N0}");

// Estadísticas
decimal total    = productos.Where(p => p.Stock > 0).Sum(p => p.Precio * p.Stock);
int     sinStock = productos.Count(p => p.Stock == 0);

// Agrupar
var grupos = productos
    .GroupBy(p => p.Stock > 0 ? "disponible" : "sin stock")
    .ToDictionary(g => g.Key, g => g.ToList());
```

> **Lazy evaluation:** las consultas LINQ no se ejecutan al definirlas — se ejecutan al iterar el resultado (`foreach`, `ToList()`, `Count()`, etc.). Esto se llama **evaluación diferida**.

---

## 5. Dictionary\<TKey, TValue\>

Un `Dictionary<TKey, TValue>` es una colección de **pares clave → valor** con acceso en O(1). Internamente usa una tabla hash.

### 5.1 Creación

```csharp
// Inicializador con sintaxis de índice (recomendado)
var telefonos = new Dictionary<string, string>
{
    ["Ana"]    = "381-123-4567",
    ["Carlos"] = "381-987-6543",
    ["Laura"]  = "381-555-0000"
};

// Desde lista de objetos con LINQ
var porNombre = productos.ToDictionary(p => p.Nombre);
var porClave  = productos.ToDictionary(p => p.Nombre, p => p.Precio);
```

### 5.2 Acceso

```csharp
// Acceso directo — lanza KeyNotFoundException si la clave no existe
string numero = telefonos["Ana"];

// TryGetValue — acceso seguro (recomendado en producción)
if (telefonos.TryGetValue("Pedro", out var tel))
    Console.WriteLine($"Teléfono: {tel}");

// GetValueOrDefault — valor de fallback
var numero2 = telefonos.GetValueOrDefault("Pedro", "Sin número");

bool existe = telefonos.ContainsKey("Ana");
```

### 5.3 Modificación

```csharp
telefonos.Add("Diego", "381-111-2222");      // lanza si ya existe
telefonos["Ana"]  = "381-999-8888";          // add o update
telefonos.TryAdd("Ana", "381-000-0000");     // false, ya existe
telefonos.Remove("Carlos");
```

### 5.4 Recorrido

```csharp
// Desestructuración del par clave/valor
foreach (var (nombre, telefono) in telefonos)
    Console.WriteLine($"{nombre}: {telefono}");

// LINQ
var ordenados = telefonos
    .OrderBy(kv => kv.Key)
    .Select(kv => $"{kv.Key} → {kv.Value}");
```

### 5.5 Variantes útiles

| Tipo | Diferencia |
|------|-----------|
| `Dictionary<K,V>` | Estándar, sin garantía de orden |
| `SortedDictionary<K,V>` | Claves ordenadas — O(log n) |
| `ConcurrentDictionary<K,V>` | Seguro para acceso multi-hilo |
| `OrderedDictionary<K,V>` | Mantiene orden de inserción — .NET 9+ |

---

## 6. Record

Un `record` es un tipo especial introducido en C# 9 diseñado para representar **datos con identidad semántica**. El compilador genera automáticamente: constructor, propiedades, `Equals`, `GetHashCode`, `ToString` y deconstrucción.

### 6.1 Diferencia clave con class

```csharp
// Con class — igualdad por referencia
class PersonaClass { public string Nombre; public int Edad; }

var c1 = new PersonaClass { Nombre = "Ana", Edad = 25 };
var c2 = new PersonaClass { Nombre = "Ana", Edad = 25 };
Console.WriteLine(c1 == c2); // False — dos objetos distintos en memoria

// Con record — igualdad por valor
record PersonaRecord(string Nombre, int Edad);

var r1 = new PersonaRecord("Ana", 25);
var r2 = new PersonaRecord("Ana", 25);
Console.WriteLine(r1 == r2); // True — mismo contenido = iguales
```

### 6.2 Declaración

```csharp
// Posicional — la forma más compacta (recomendada)
record Persona(string Nombre, int Edad);

// Con cuerpo — permite agregar miembros calculados
record Punto(double X, double Y)
{
    public double Distancia => Math.Sqrt(X * X + Y * Y);
    public Punto Trasladar(double dx, double dy) => this with { X = X + dx, Y = Y + dy };
}

// Record struct — vive en el stack (para datos pequeños y de alto rendimiento)
readonly record struct Color(byte R, byte G, byte B);

// Record class — equivalente a record, sintaxis explícita
record class Producto(string Nombre, decimal Precio, int Stock);
```

### 6.3 Instanciación y ToString

```csharp
var p1 = new Persona("Ana", 25);

Console.WriteLine(p1.Nombre);  // Ana
Console.WriteLine(p1.Edad);    // 25
Console.WriteLine(p1);         // Persona { Nombre = Ana, Edad = 25 }
```

### 6.4 Expresión `with` — copiar con cambios

```csharp
var original   = new Persona("Ana", 25);
var cumpleaños = original with { Edad = 26 };

Console.WriteLine(original.Edad);   // 25 — no se modificó
Console.WriteLine(cumpleaños.Edad); // 26
```

> Este patrón se llama **copy-on-write** y es fundamental en arquitecturas inmutables y funcionales.

### 6.5 Deconstrucción

```csharp
var persona = new Persona("Ana", 25);
var (nombre, edad) = persona;

Console.WriteLine($"{nombre} tiene {edad} años");
```

### 6.6 Herencia y pattern matching

```csharp
abstract record Figura(string Color);
record Circulo(string Color, double Radio)                  : Figura(Color);
record Rectangulo(string Color, double Ancho, double Alto)  : Figura(Color);
record Triangulo(string Color, double Base, double Altura)  : Figura(Color);

static double CalcularArea(Figura fig) => fig switch
{
    Circulo    { Radio: var r }               => Math.PI * r * r,
    Rectangulo { Ancho: var a, Alto: var h }  => a * h,
    Triangulo  { Base: var b, Altura: var h } => b * h / 2,
    _                                         => throw new ArgumentException($"Figura desconocida: {fig}")
};
```

### 6.7 Guía de uso

| Contexto | Tipo recomendado |
|---------|----------------|
| Retorno temporal de un método | Tupla |
| DTO entre capas (API, servicio, repositorio) | `record` |
| Mensaje de dominio / evento | `record` |
| Resultado con éxito/error | `record` con herencia |
| Entidad con estado mutable | `class` |
| Dato pequeño y de alto rendimiento | `readonly record struct` |

---

## 7. Tipos Anónimos

Un **tipo anónimo** es una clase generada automáticamente por el compilador para agrupar un conjunto de propiedades de sólo lectura, sin necesidad de declarar un tipo con nombre. Son de uso exclusivo dentro del método donde se crean.

### 7.1 Creación

```csharp
// El compilador genera internamente una clase con propiedades Nombre y Edad
var persona = new { Nombre = "Ana", Edad = 25 };

Console.WriteLine(persona.Nombre); // Ana
Console.WriteLine(persona.Edad);   // 25
Console.WriteLine(persona);        // { Nombre = Ana, Edad = 25 }
```

La variable **debe** declararse con `var` porque el nombre del tipo generado no es accesible en el código fuente.

### 7.2 Inferencia de nombres de propiedad

Si el valor proviene de una variable o propiedad existente, el nombre se infiere automáticamente:

```csharp
string nombre = "Carlos";
int    edad   = 30;

// Las propiedades toman el nombre de las variables
var p = new { nombre, edad };

Console.WriteLine(p.nombre); // Carlos
Console.WriteLine(p.edad);   // 30
```

### 7.3 Igualdad estructural

Dos tipos anónimos con las mismas propiedades (mismo nombre, tipo y orden) son considerados el mismo tipo por el compilador y su igualdad se compara por valor:

```csharp
var a = new { Nombre = "Ana",  Edad = 25 };
var b = new { Nombre = "Ana",  Edad = 25 };
var c = new { Nombre = "Luis", Edad = 30 };

Console.WriteLine(a.Equals(b)); // True
Console.WriteLine(a.Equals(c)); // False
```

> A diferencia de `record`, el operador `==` no está sobrecargado para tipos anónimos. Usar `.Equals()` para comparar.

### 7.4 Caso de uso principal — proyecciones LINQ

Los tipos anónimos están especialmente diseñados como destino de `Select` en LINQ cuando sólo se necesita un subconjunto de campos y no vale la pena crear un `record` dedicado:

```csharp
record Empleado(string Nombre, string Departamento, decimal Salario, int Antiguedad);

List<Empleado> empleados =
[
    new("Ana",    "IT",       95_000, 5),
    new("Carlos", "IT",       87_000, 3),
    new("Laura",  "RRHH",     72_000, 8),
    new("Diego",  "RRHH",     68_000, 2),
    new("Sofía",  "Finanzas", 110_000, 10),
];

// Proyección — el tipo anónimo contiene sólo lo que necesitamos
var resumen = empleados
    .Where(e => e.Salario > 70_000)
    .OrderByDescending(e => e.Salario)
    .Select(e => new
    {
        e.Nombre,
        e.Departamento,
        SalarioFormateado = $"${e.Salario:N0}",
        Seniority = e.Antiguedad >= 5 ? "Senior" : "Junior"
    });

foreach (var item in resumen)
    Console.WriteLine($"{item.Nombre,-10} | {item.Departamento,-10} | {item.SalarioFormateado,12} | {item.Seniority}");
```

### 7.5 Colecciones de tipos anónimos

```csharp
var puntos = new[]
{
    new { X = 0, Y = 0 },
    new { X = 3, Y = 4 },
    new { X = 1, Y = 1 },
};

var cercanos = puntos
    .Where(p => p.X + p.Y < 5)
    .OrderBy(p => p.X);
```

### 7.6 Limitaciones importantes

| Limitación | Alternativa |
|-----------|------------|
| No se puede usar como parámetro de método tipado | `record` o clase |
| No se puede retornar desde un método tipado | `record` o tupla |
| No se puede usar fuera del método donde se creó | `record` |
| Propiedades de sólo lectura — no se pueden modificar | `record` con `with` |

```csharp
var punto = new { X = 1, Y = 2 };
punto.X = 5;   // Error de compilación: Property cannot be assigned to — it is read only
```

### 7.7 Tipos anónimos vs. tuplas vs. records

| Característica | Tipo anónimo | Tupla | Record |
|---------------|:---:|:---:|:---:|
| Nombres de campo | Sí | Opcional | Sí |
| Igualdad por valor | Sí (`.Equals`) | Sí (`==`) | Sí (`==`) |
| Usable fuera del método | No | Sí | Sí |
| Retornable desde método tipado | No | Sí | Sí |
| Herencia | No | No | Sí |
| Expresión `with` | No | Parcial | Sí |
| Uso principal | Proyecciones LINQ | Retorno múltiple | DTOs / Dominio |

---

## Resumen comparativo

```
Necesito manipular o construir texto
    └─► string  /  StringBuilder (si hay concatenaciones en loop)

Necesito agrupar datos temporalmente en un método
    └─► Tupla

Necesito una secuencia de tamaño conocido en compilación
    └─► Array T[]

Necesito agregar/quitar elementos dinámicamente
    └─► List<T>

Necesito buscar por clave en O(1)
    └─► Dictionary<TKey, TValue>

Necesito un dato con identidad semántica e igualdad por valor
    └─► record

Necesito proyectar campos temporalmente en una consulta LINQ
    └─► Tipo anónimo
```
