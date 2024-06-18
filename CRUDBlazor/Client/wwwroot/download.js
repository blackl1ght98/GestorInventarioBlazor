

//Cuando la pagina web se cargue por completo...
window.onload = function () {
    //Abre en una nueva ventana una funcion llamada saveAsFile que recibe el nombre del archivo y un array de bytes
    window.saveAsFile = function (filename, bytesBase64) {
        //Crea el enlace del html
        var link = document.createElement('a');
        //Al nombre del archivo le asigna el link de descarga
        link.download = filename;
        //A ese link se codifica en base 64
        link.href = "data:application/octet-stream;base64," + bytesBase64;
        //Al cuerpo del documento agrega el link
        document.body.appendChild(link); 
        //Cuando haga click el usuario
        link.click();
        //El link generado es eliminado
        document.body.removeChild(link);
    }
}

