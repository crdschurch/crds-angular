# Teardown/Reload Test Data

This folder contains scripts for deleting pre-configured (aka. "scripted") test data and re-loading it back into Ministry Platform. These scripts are run from a TeamCity [Teardown/Reload](https://ci.crossroads.net/viewType.html?buildTypeId=Qa_Integration_TeardownReloadTestData) build after a database refresh is done. This build must be kicked off manually.

## Relocation in progress!

The process to teardown and reload test data is being migrated to the [ManageTestData](https://github.com/crdschurch/automated-e2e-tests/tree/master/CrdsManageTestData/ManageTestData) project in the automated-e2e-tests repo. The Powershell scripts and CSV config files in this folder are __not__ being used or maintained and will be removed soon.

The SQL scripts in this folder are still being maintained.

## SQL Stored Procedures

The StandardScripts directory holds SQL scripts for stored procedures used to add or removed test data configured by CSV files.

The CustomScripts directory hold SQL scripts needed to process data outside what has been configured.

The Teardown/Reload build runs in this order:
1. Scripts in StandardScripts are run - this loads all th stored procedures into the database. Teardown scripts are loaded first, then reload scripts.
2. Scripts in CustomScripts/Teardown are run.
3. The Teardown/Reload project and custom scripts are run - leveraging stored procedures loaded in step 1 to delete test data, then re-add it.
4. Scripts in CustomScripts/Reload are run.


# Troubleshooting

How to fix some errors you might see logged during the TeamCity build.

## "DELETE statement conflicted with REFERENCE constraint"
ex: "The DELETE statement conflicted with the REFERENCE constraint "FK_Contribution_Statement_Donors_Donors". The conflict occurred in database "MinistryPlatform", table "dbo.Contribution_Statement_Donors", column 'Donor_ID'."

First find the source:
1. Identify the key values in the error message:
  - `Table` = Contribution_Statement_Donors
  - `Key Constraint` = FK_Contribution_Statement_Donors_Donors
2. In SQL Server, navigate to the `Table` ("Contribution_Statement_Donors") and expand the "Keys" folder. Find and open the `Key Constraint` ("FK_Contribution_Statement_Donors_Donors"). You'll probably trigger a popup warning you are not an admin - click OK.
3. The Foreign Key Relationships popup should be open to the `Key Constraint`. Expand the "Tables and Columns Specifications" section and note the "Primary/Unique Key Base Table" value ("Donors"). Let's call this table the `Source Table`. The error was caused when someone tried to delete an entry from this table before deleting any entries in the `Table` from the error ("Contribution_Statement_Donors") related to it.

Next patch it:
1. Find the SQL script that handles deleting records from the `Source Table`. The script will be found in the [TeardownStoredProcs](StandardScripts/00.TeardownStoredProcs/) folder. Have that file open and ready for editing.
2. Run the [MatchForeignKeysToPrimaryKeys](https://github.com/crdschurch/automated-e2e-tests/blob/master/SQLScripts/ManualTeardownScripts/HelpersForTDRLCreation/MatchForeignKeysToPrimaryKeys.sql) helper script, setting the @PrmaryTableName variable to the `Source Table`. The script should report two result tables:
- `Table A` where all "PK Table" values = `Source Table`
- `Table B` where all "Table w FK" values = `Source Table` 
3. Note all the tables listed in the "FK Table" column in `Table A`. Foreign Keys from these Tables must be deleted ("FK is Nullable" = 0) or set to null ("FK is Nullable" = 0) before the entry in the `Source Table` can be safely deleted. To fix the error, find the "FK Table" entry that is not being deleted or set to null by the deletion script and add it.
4. Run the updated Stored Procedure creation script to update the script in the DB, then test to confirm the change worked, and create a PR with the updated script.