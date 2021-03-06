Introduction
------------

This project is a fork of "ye olde trusty" Migrator.Net - the original project can be found [here on google code][1], and has since [moved to github][2]
  
What's different in the fork
----------------------------

In this fork the main changes are:

* Now targets .Net Framework 4.0 instead of 2.0/3.5.
* Support for reserved words.
* Support for guid types across all databases.
* Utility classes for removing all tables etc. from a database (to support migration integration tests).
* Warnings in Oracle when attempting to create column/table/key names that are over-length.
* Removed reliance on deprecated SqlServer views such as Sysconstraints (to support Azure Sql Server deployment).

  [1]: http://code.google.com/p/migratordotnet/
  [2]: https://github.com/migratordotnet/Migrator.NET