using System.Windows;

namespace ui_zadanie4
{
	public partial class MainWindow
	{
		private void FamilyButton_OnClick(object sender, RoutedEventArgs e)
		{
#if ZatvorkyPreFakty
			Memory.Text = @"(Peter je rodic Jano)
(Peter je rodic Vlado)
(manzelia Peter Eva)
(Vlado je rodic Maria)
(Vlado je rodic Viera)
(muz Peter)
(muz Jano)
(muz Vlado)
(zena Maria)
(zena Viera)
(zena Eva)";
#else
			Memory.Text = @"Peter je rodic Jano
Peter je rodic Vlado
manzelia Peter Eva
Vlado je rodic Maria
Vlado je rodic Viera
muz Peter
muz Jano
muz Vlado
zena Maria
zena Viera
zena Eva";
#endif

			Rules.Text = @"DruhyRodic1:
AK ((?X je rodic ?Y)(manzelia ?X ?Z))
POTOM ((pridaj ?Z je rodic ?Y))

DruhyRodic2:
AK ((?X je rodic ?Y)(manzelia ?Z ?X))
POTOM ((pridaj ?Z je rodic ?Y))

Otec:
AK ((?X je rodic ?Y)(muz ?X))
POTOM ((pridaj ?X je otec ?Y))

Matka:
AK ((?X je rodic ?Y)(zena ?X))
POTOM ((pridaj ?X je matka ?Y))

Surodenci:
AK ((?X je rodic ?Y)(?X je rodic ?Z)(<> ?Y ?Z))
POTOM ((pridaj ?Y a ?Z su surodenci))

Brat:
AK ((?Y a ?Z su surodenci)(muz ?Y))
POTOM ((pridaj ?Y je brat ?Z))

Stryko:
AK ((?Y je brat ?Z)(?Z je rodic ?X))
POTOM ((!pridaj ?Y je stryko ?X)(sprava ?X ma stryka))

Test mazania:
AK ((?Y je stryko ?X)(zena ?X))
POTOM ((vymaz zena ?X))";
		}

		private void Fiats2Button_OnClick(object sender, RoutedEventArgs e)
		{
#if ZatvorkyPreFakty
			Memory.Text = "(start)";
#else
			Memory.Text = "start";
#endif
			Rules.Text = @"p1:
AK ((start)(typ karoserie ?sedan-hatchback))
POTOM ((pridaj karoseria ?sedan-hatchback)(vymaz start))";

#if ZatvorkyPreFakty
			Rules.Text += @"
help: AK((start)) POTOM (
	(sprava (typ karoserie ?sedan-hatchback))
	(sprava (predna maska ?ano-nie-mriezka))
	(sprava (ma ?okruhle-integrovane svetla))
	(sprava (pohanana naprava ?predna-zadna))
	(sprava ak sedan:)
		(sprava 	(pocet dveri ?4-5))
	(sprava ak hatchback:)
		(sprava 	(pocet dveri ?3-5))
)
";
#else
			Rules.Text += @"
help: AK((start)) POTOM (
	(sprava typ karoserie ?sedan-hatchback)
	(sprava predna maska ?ano-nie-mriezka)
	(sprava ma ?okruhle-integrovane svetla)
	(sprava pohanana naprava ?predna-zadna)
	(sprava (ak sedan))
		(sprava pocet dveri ?4-5)
	(sprava (ak hatchback))
		(sprava pocet dveri ?3-5)
)
";
#endif

			Rules.Text += @"
p2:
AK ((karoseria sedan)(pocet dveri ?4-5))
POTOM ((pridaj sedan ?4-5))

p3:
AK ((sedan 5))
POTOM ((pridaj vybrany Fiat Croma)(sprava Fiat Croma))

p4:
AK ((sedan 4)(pohanana naprava ?predna-zadna))
POTOM ((pridaj naprava ?predna-zadna))

p5:
AK ((naprava predna))
POTOM ((pridaj vybrany Fiat Tempra)(sprava Fiat Tempra))

p6:
AK ((naprava zadna))
POTOM ((pridaj vybrany Fiat Mirafiorri)
	   (sprava Fiat Mirafiorri))

p7:
AK ((karoseria hatchback)(pocet dveri ?3alebo5))
POTOM ((pridaj hatchback ?3alebo5))

p8:
AK ((hatchback 3)(predna maska ?ano-nie-mriezka))
POTOM ((pridaj 3 maska ?ano-nie-mriezka))

p9:
AK ((3 maska ano))
POTOM ((pridaj vybrany Fiat Tipo3)(sprava Fiat Tipo3))

p10:
AK ((3 maska nie))
POTOM ((pridaj vybrany Fiat Punto3)(sprava Fiat Punto3))

p11:
AK ((3 maska mriezka))
POTOM ((pridaj vybrany Fiat Panda3)(sprava Fiat Panda3))

p12:
AK ((hatchback 5)(predna maska ?ano-nie-mriezka))
POTOM ((pridaj 5 maska ?ano-nie-mriezka))

p13:
AK ((5 maska ano))
POTOM ((pridaj vybrany Fiat Tipo5)(sprava Fiat Tipo5))

p14:
AK ((5 maska nie))
POTOM ((pridaj vybrany Fiat Punto5)(sprava Fiat Punto5))

p15:
AK ((5 maska mriezka)(ma ?okruhle-integrovane svetla))
POTOM ((pridaj ?okruhle-integrovane svetla))

p16:
AK ((integrovane svetla))
POTOM ((pridaj vybrany Fiat Uno5)(sprava Fiat Uno5))

p17:
AK ((okruhle svetla))
POTOM ((pridaj vybrany Fiat Ritmo5)(sprava Fiat Ritmo5))";
		}

		private void FiatsButton_OnClick(object sender, RoutedEventArgs e)
		{
#if ZatvorkyPreFakty
			Memory.Text = @"(typ karoserie sedan)
(pocet dveri 4)
(pohanana naprava predna)
(predna maska mriezka)
(ma okruhle svetla)";
#else
			Memory.Text = @"typ karoserie sedan
pocet dveri 4
pohanana naprava predna
predna maska mriezka
ma okruhle svetla";
#endif

			Rules.Text = @"FIAT1:
AK    ((typ karoserie ?sedan_hatchback))
POTOM ((pridaj karoseria ?sedan_hatchback))

FIAT2:
AK    ((karoseria sedan)(pocet dveri ?4_5))
POTOM ((pridaj sedan ?4_5))

FIAT3:
AK    ((sedan 5))
POTOM ((pridaj vybrany Fiat Croma)(sprava Fiat Croma))

FIAT4:
AK    ((sedan 4)(pohanana naprava ?predna-zadna))
POTOM ((pridaj naprava ?predna-zadna))

FIAT5:
AK    ((naprava predna))
POTOM ((pridaj vybrany Fiat Tempra)(sprava Fiat Tempra))

FIAT6:
AK    ((naprava zadna))
POTOM ((pridaj vybrany Fiat Mirafiorri)(sprava Fiat Mirafiorri))

FIAT7:
AK    ((karoseria hatchback)(pocet dveri ?3alebo5))
POTOM ((pridaj hatchback ?3alebo5))

FIAT8:
AK    ((hatchback 3)(predna maska ?ano-nie-mriezka))
POTOM ((pridaj 3 maska ?ano-nie-mriezka))

FIAT9:
AK    ((3 maska ano))
POTOM ((pridaj vybrany Fiat Tipo3)(sprava Fiat Tipo3))

FIAT10:
AK    ((3 maska nie))
POTOM ((pridaj vybrany Fiat Punto3)(sprava Fiat Punto3))

FIAT11:
AK    ((3 maska mriezka))
POTOM ((pridaj vybrany Fiat Panda3)(sprava Fiat Panda3))

FIAT12:
AK    ((hatchback 5)(predna maska ?ano-nie-mriezka))
POTOM ((pridaj 5 maska ?ano-nie-mriezka))

FIAT13:
AK    ((5 maska ano))
POTOM ((pridaj vybrany Fiat Tipo5)(sprava Fiat Tipo5))

FIAT14:
AK    ((5 maska nie))
POTOM ((pridaj vybrany Fiat Punto5)(sprava Fiat Punto5))

FIAT15:
AK    ((5 maska mriezka)(ma ?okruhle-integrovane svetla))
POTOM ((pridaj ?okruhle-integrovane svetla))

FIAT16:
AK    ((integrovane svetla))
POTOM ((pridaj vybrany Fiat Uno5)(sprava Fiat Uno5))

FIAT17:
AK    ((okruhle svetla))
POTOM ((pridaj vybrany Fiat Ritmo5)(sprava Fiat Ritmo5))";
		}
	}
}