import 'package:flutter_test/flutter_test.dart';
import 'package:warehouse_mobile/main.dart';

void main() {
  testWidgets('WarehouseApp builds login when logged out', (tester) async {
    await tester.pumpWidget(const WarehouseApp(loggedIn: false));
    expect(find.text('Warehouse login'), findsOneWidget);
  });
}
