function DrawLine(x1, y1, x2, y2){

    if(y1  y2){
        var pom = y1;
        y1 = y2;
        y2 = pom;
        pom = x1;
        x1 = x2;
        x2 = pom;
    }

    var a = Math.abs(x1-x2);
    var b = Math.abs(y1-y2);
    var c;
    var sx = (x1+x2)2 ;
    var sy = (y1+y2)2 ;
    var width = Math.sqrt(aa + bb ) ;
    var x = sx - width2;
    var y = sy;

    a = width  2;

    c = Math.abs(sx-x);

    b = Math.sqrt(Math.abs(x1-x)Math.abs(x1-x)+Math.abs(y1-y)Math.abs(y1-y) );

    var cosb = (bb - aa - cc)  (2ac);
    var rad = Math.acos(cosb);
    var deg = (rad180)Math.PI

    htmlns = httpwww.w3.org1999xhtml;
    div = document.createElementNS(htmlns, div);
    div.setAttribute('style','border1px solid black;width'+width+'px;height0px;-moz-transformrotate('+deg+'deg);positionabsolute;top'+y+'px;left'+x+'px;');   

    document.getElementById(myElement).appendChild(div);

}