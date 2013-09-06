function configureTrailerButton() {
    $('.trailerButton').click(function () {
        var that = this;

        var $button = $(that);

        if ($button.hasClass('pushed')) {
            var $trailerRows = $('.trailerContainerRow');
            $trailerRows.remove();

            $button.removeClass('pushed');
        } else {
            $.ajax({
                url: $(that).attr('data-trailer-url'),
                context: that
            }).done(function(data) {
                $('.trailerButton.pushed').removeClass('pushed');

                var $trailerRows = $('.trailerContainerRow');
                $trailerRows.remove();

                var $button = $(this);

                var $tr = $($button.closest('tr'));
                $tr.after('<tr class="trailerContainerRow"><td colspan="4">' + data + '</td></tr>');

                $button.addClass('pushed');
            });
        }
    });
}