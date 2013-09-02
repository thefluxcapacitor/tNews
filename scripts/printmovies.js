db = connect("localhost:27017/torrentnews");
cursor = db.movies.find();
while(cursor.hasNext()){
    printjson(cursor.next());
}