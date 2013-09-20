db = connect("localhost:27017/torrentnews");
cursor = db.users.find();
while(cursor.hasNext()){
    printjson(cursor.next());
}