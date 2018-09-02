ApiSandbox Routes

List all collections in database 
	ApiSandbox/dbc/list

List Loaded Collection info
	ApiSandbox/col/list

Load a collection from db, db_Coll_Id in database, loaded name coll_name
	ApiSandbox/dbc/load/{db_Coll_Id}/{coll_name}

Return an already loaded collection 
	ApiSandbox/col/{collection}

Return a document from a loaded collection
	ApiSandbox/col/{collection}/{id}

Set in memory collection, 
POST	ApiSandbox/col/{collection}

Update in memory collection data
PUT		ApiSandbox/col/{collection}/{id}

Load group of collections from database
	ApiSandbox/dbc/gload/{group_name}

Delete group of collections from database
	ApiSandbox/dbc/gdelete/{group_name}

Delete collection from database
	ApiSandbox/dbc/delete/{name}

Save in memory collection to database
	ApiSandbox/dbc/save/{coll_name}/{target_name}

Load collection data
	ApiSandbox/{collection}/bulkload