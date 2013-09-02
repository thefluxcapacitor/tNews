db = connect("localhost:27017/torrentnews");
cursor = db.torrents.find();
while(cursor.hasNext()){
    printjson(cursor.next());
}