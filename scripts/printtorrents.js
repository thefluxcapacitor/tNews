db = connect("localhost:27017/torrentnews");
cursor = db.torrents.find().sort({Score:-1});
while(cursor.hasNext()){
    printjson(cursor.next());
}