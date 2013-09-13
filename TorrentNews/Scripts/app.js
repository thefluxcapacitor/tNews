function configureTrailerButton() {
    $('.trailerButton').click(function () {
        var that = this;

        var $button = $(that);

        if ($button.hasClass('pushed')) {
            var $trailerRows = $('.trailerContainerRow');
            $trailerRows.remove();

            $button.removeClass('pushed');
        } else {
            $('.trailerButton.pushed').removeClass('pushed');

            var $trailerRows = $('.trailerContainerRow');
            $trailerRows.remove();

            $.ajax({
                url: $(that).attr('data-trailer-url'),
                context: that
            }).done(function(data) {
                var $button = $(this);

                var $tr = $($button.closest('tr'));
                $tr.after('<tr class="trailerContainerRow"><td colspan="4">' + data + '</td></tr>');

                $('.closeTrailer').click(function () {
                    $('.trailerButton.pushed').removeClass('pushed');

                    var $trailerRows = $('.trailerContainerRow');
                    $trailerRows.remove();

                    var torrentId = $(this).attr('data-torrent-id');
                    $.scrollTo($('tr[data-torrent-id=' + torrentId + ']'), 700);
                });

                $.scrollTo($('.trailerContainerRow'), 1700);

                $button.addClass('pushed');
            });
        }
    });
}

function configureTooltips() {
    $('.hasTooltip').hover(function () {
        var $that = $(this);
        $that.next('.tooltip').show(200);
    }, function () {
        var $that = $(this);
        $that.next('.tooltip').hide(0);
    });
}