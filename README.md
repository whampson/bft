# Cascara
[![Build Status](
    https://travis-ci.org/whampson/cascara.svg?branch=master)
](https://travis-ci.org/whampson/cascara)

**Cascara** is a C# library for reading and editing binary files. Cascara
provides a simple framework for accessing fields in a binary file via the use of
Layout Files.

## Features
### Layout File
(TODO: element attributes, variables)

A **Layout File** is an XML file that defines the structure of a binary file.
Layout files are constructed using a fixed set of XML elements that, when
combined, can be used to create complex data structures. Data type instances
can be repeated to create arrays and named to become accessible by your program.

### List of XML Elements
#### Root Element
Name                    | Description
----------------------- | ------------
`cascaraBinaryTemplate` | All Layout Files must open with this element.

#### Data Types
**Integer Types**  
Name     | Description
-------- | ------------
`byte`   | alias for `uint8`
`char`   | 8-bit integer value; intended for use with characters
`char8`  | 16-bit integer value; intended for use with characters
`char16` | 32-bit integer value; intended for use with characters
`int`    | alias for `int32`
`int8`   | 8-bit signed integer value
`int16`  | 16-bit signed integer value
`int32`  | 32-bit signed integer value
`int64`  | 64-bit signed integer value
`long`   | alias for `int64`
`short`  | alias for `int16`
`uint`   | alias for `uint32`
`uint8`  | 8-bit unsigned integer value
`uint16` | 16-bit unsigned integer value
`uint32` | 32-bit unsigned integer value
`uint64` | 64-bit unsigned integer value
`ulong`  | alias for `uint64`
`ushort` | alias for `uint16`

**Boolean Types**  
A *Boolean* is defined as being `false` for the value 0 and `true` for any non-zero value.
Name     | Description
-------- | ------------
`bool`   | alias for `bool8`
`bool8`  | 8-bit Boolean value
`bool16` | 16-bit Boolean value
`bool32` | 32-bit Boolean value

**Floating-Point Types**  
Name     | Description
-------- | ------------
`float`  | alias for `single`
`single` | 32-bit IEEE-754 floating-point value
`double` | 64-bit IEEE-754 floating-point value

**Structure Types**  
Name     | Description
-------- | ------------
`struct` | a data structure whose members are laid out sequentially in memory
`union`  | a data structure whose members start at the same address

#### Directives
Name      | Description
--------- | ------------
`align`   | adjusts the current offset
`echo`    | prints a string to the current output stream
`include` | introduces definitions from other Layout Files into the global namespace
`local`   | declares a variable which is not a part of the binary file data
`typedef` | defines a new data type

### Deserialization
Layout Files can be used to deserialize a binary file into an object.


## Example Usage
### Layout File
An example Layout File. The offset of each variable is shown in the table below.
```xml
<cascaraLayout description="Player info">
    <!-- You can use 'typedef' to create custom data types -->
    <typedef name="Vect3D" kind="struct">
        <float name="X"/>
        <float name="Y"/>
        <float name="Z"/>
    </typedef>

    <!-- File data starts here -->
    <uint8 name="Health"/>
    <uint8 name="Armor"/>
    <align count="2" comment="These bytes are garbage data"/>
    <Vect3D name="Position"/>
    <struct name="Statistics">
        <int32 name="TimesDied"/>
        <int32 name="NumCollectiblesFound"/>
        <bool32 name="HasUsedCheats"/>
    </struct>
</cascaraLayout>
```

Offset | Type   | Name
------ | ----   | ----
0x00   | uint8  | Health
0x01   | uint8  | Armor
0x04   | float  | Position.X
0x08   | float  | Position.Y
0x0C   | float  | Position.Z
0x10   | int32  | Statistics.TimesDied
0x14   | int32  | Statistics.NumCollectiblesFound
0x18   | bool32 | Statistics.HasUsedCheats
0x1C   | -      | \<EOF>


### Reading/Writing Data
#### Reading and Writing by Direct Data Manipulation

```c#
using WHampson.Cascara;
using WHampson.Cascara.Types;

struct Point3D
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
}

// Open a binary file
BinaryFile bFile = BinaryFile.Open("my_file.bin");

// Read layout file
bFile.ReadLayout("layout.xml");

// Get fields from file
byte armor = bFile.GetValue<byte>("Armor");
Point3D position = bFile.GetValue<Point3D>("Position");

// Set fields
bFile.SetValue<byte>("Health", 255);
bFile.SetValue<Bool32>("Statistics.HasUsedCheats", true);

// Write and close
bFile.Write("my_file.bin");
bFile.Close();
```

#### Reading by Deserialization
```c#
using WHampson.Cascara;
using WHampson.Cascara.Types;

using (BinaryFile bFile = BinaryFile.Open("my_file.bin"))
{
    bFile.ApplyTemplate("layout.xml");

    object o = bFile.Extract<object>();
}
```

## Credits
Copyright (C) 2017 Wes Hampson.
