# ipc-poc


Common

Data
	File
		id, guid, FileName, Path
	Customer
		id, guid, Name, Email, Phone, Birtdate
						{number}@email.com

--

Client.Web					10001
	/Home/Index 
		* a link to the FileClient
		* a butoon getting random customer from cache service
		* a butoon inserting customer example data to business service
	
	UploadFile()
		

Client.File					10011
	/File/Get/{guid}
	
	/files/{guid}.txt
		
--

Server.Cache				10021
	LoadAllCustomerToMemory() key - email
	GetCustomerFromMemoryByEmail()	
	
	
Server.Business				10031
	FillDummyDataToDB(numberOfRecord)
	
	GetCustomerFromMemoryByEmail()		
	GetCustomerFromDBByEmail()	
	InsertCustomerToDBAndCache()
	
	
--
	
DBServer (postgresql)		5432
	has 1 milion rows
