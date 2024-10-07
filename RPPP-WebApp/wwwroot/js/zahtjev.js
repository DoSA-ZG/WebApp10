$(document).on('click', '.deleterow', function () {
    event.preventDefault();
    var tr = $(this).parents("tr");
    tr.remove();
    clearOldMessage();
});

$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });



    $("#zadatak-dodaj").click(function (event) {
        event.preventDefault();
        dodajZadatak();
    });
});

function dodajZadatak() {
    var id = $("#zadatak-id").val();

        var status = $("#zadatak-status").val()

        var aktivan = $("#zadatak-aktivan").val()


        var opis = $("#zadatak-opis").val();
        var template = $('#template').html();
        var idZahtjev = $("#zadatak-idZahtjev").val();
        var nositelj = $("#zadatak-nositelj").val();
        var idPrioritet = $("#zadatak-idPrioritet").val();



    template = template.replace(/--id--/g, id)
        .replace(/--status--/g, status)
            .replace(/--aktivan--/g, aktivan)
        .replace(/--opis--/g, opis)
        .replace(/--IdZahtjev--/g, idZahtjev)
        .replace(/--nositelj--/g, nositelj)
        .replace(/--IdPrioritet--/g, idPrioritet);
        $(template).find('tr').insertBefore($("#table-zadaci").find('tr').last());

    $("#zadatak-id").val('');
    $("#zadatak-status").val('');
    $("#zadatak-aktivan").val('');
    $("#zadatak-opis").val('');
    $("#zadatak-idZahtjev").val('');
    $("#zadatak-nositelj").val('');
    $("#zadatak-idPrioritet").val('');

        clearOldMessage();
 
}