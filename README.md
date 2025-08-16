# AcumaticaExecSQL
Allows a user to execute a limited set of SQL queries directly against the database and have results returned as a html table

# Installation
Customization zip in the Install folder

# Usage
Write queries in the Query text area and click "Run Query" to execute the query. Results will be returned in a table below the query area.

# Security
Since this customization allows direct SQL execution, it should be used with caution. Ensure that only trusted users have access to this functionality. It is recommended to limit the queries that can be executed to prevent potential security risks. An attempt is made to sanitise SQL however this is for testing purposes alone

# Limitations
This customization is intended for testing and debugging purposes only. It is not recommended for use in production environments due to potential security risks and performance issues. The customization may not support complex queries or large datasets. It is also not designed to handle transactions or provide advanced SQL features. Use at your own risk. 

# Tips
- Start with simple SELECT queries to retrieve data from the database.
- Limit the scope by using CompanyID in the WHERE clause.
- Use Top() to limit the number of rows returned.


