$(function () {
	$('head').append('<link rel="stylesheet" href="css/style-0.css" rel="stylesheet">');
	$('.js-colorswatch').on('click',function (e){
		$('.colorswatch').toggleClass('opened');
	})
	$('.js-swatch-color').on('click',function (e){
		var color = $(this).attr('data-color');
		if(color != undefined){
			$('link[href*="css/style-"]').not('link[href*="css/style-rtl"]').attr('href','css/style-color-'+color+'.css');
//			$('.header-logo img').attr('src','images/logo-color-'+color+'.png');
//			$('.footer-logo img').attr('src','images/footer-logo-color-'+color+'.png');
		} else {
			$('link[href*="css/style-"]').attr('href','css/style-0.css');
//			$('.header-logo img').attr('src','images/logo.png');
//			$('.footer-logo img').attr('src','images/footer-logo.png');
		}
		$('.js-swatch-color').removeClass('active');
		$(this).toggleClass('active');
		e.preventDefault();
	});
//	$('.js-rtl').on('change',function (e){
//		if ($('body').hasClass('rtl')){
//			$('#rtlStyle').remove();
//			$('body').removeClass('rtl');
//			$(this).removeClass('active');
//		} else {
//			$('body').addClass('rtl');
//			$(this).addClass('active');
//			$('head').append('<link href="css/style-rtl.css" rel="stylesheet" id="rtlStyle">');
//		}
//		e.preventDefault();
//	});
})