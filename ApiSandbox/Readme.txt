ApiSandbox Routes

List all collections in database 
	ApiSandbox/listdb

List Loaded Collection info
	ApiSandbox/listloaded

Load a collection from db, db_Coll_Id in database, loaded name coll_name
	ApiSandbox/load/{db_Coll_Id}/{coll_name}

Return an already loaded collection 
	ApiSandbox/{collection}

Return a document from a loaded collection
	ApiSandbox/{collection}/{id}

Set in memory collection, 
POST	ApiSandbox/{collection}

Update in memory collection data
PUT		ApiSandbox/{collection}/{id}

Load group of collections from database
	ApiSandbox/loadgroup/{group_name}

Delete group of collections from database
	ApiSandbox/deletegroup/{group_name}

Save group of collections to db
	savegroup/group_name

Delete collection from database
	ApiSandbox/dbc/delete/{name}

Save in memory collection to database
	ApiSandbox/save/{coll_name}/{target_name}

Load collection data
	ApiSandbox/bulkload/{collection}