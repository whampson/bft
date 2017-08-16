# Cascara
[![Build Status](
    https://travis-ci.org/whampson/cascara.svg?branch=master)
](https://travis-ci.org/whampson/cascara)

**Cascara** is a C# library for reading and editing binary files. Cascara
provides a simple framework for accessing fields in a binary file via the use of
Layout Files.

### Layout File
A **Layout File** is an XML file that defines the structure of a binary file.
Layout files are constructed using a fixed set of XML elements that, when
combined, can be used to create create complex data structures. Data type
instances can be repeated to create arrays and named to become accessible by
your program.

For a full list of elements that can be used in a Layout File, check out the
LINK(wiki).

#### Example Layout File
Say we want to create a layout for a file containing four points in 3-space. If
each position vector is stored as a set of three 32-bit floating-point numbers,
each  <code>float</code> representing a component of a vector, we can use the
following layout:
```xml
<cascaraLayout>
    <struct name="MyVect3D" count="4">
        <float name="X"/>
        <float name="Y"/>
        <float name="Z"/>
    </struct>
</cascaraLayout>
```

TODO: finish!
