# AOS Installtion
#### Requirements
* Ubuntu 16.04
#### Install dependencies
* [MongoDB](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-ubuntu/) 
* [Postman](https://www.postman.com/downloads/)
* Load DB (run this command from the AOS.archive directory)</br>
`mongorestore -v --nsFrom "AOS.*" --nsTo "AOS.*" --uri="mongodb://localhost:27017/" --archive="AOS.archive"`

#### Download code:
* Download the AOS Web API </br>
`git clone https://github.com/orhaimwerthaim/AOS-WebAPI` 

* Download the Planning Engine base into your <_planning engine directory_> </br>
`git clone https://github.com/orhaimwerthaim/AOS-Solver`

...
