import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../l10n/locale_controller.dart';
import '../scan_go/self_checkout_api.dart';
import '../theme/modern_tech_theme.dart';
import 'login_screen.dart';
import 'scan_go_cart_screen.dart';

/// Guest-only entry for Scan & Go. Staff login is a discreet secondary action.
class ScanGoWelcomeScreen extends StatefulWidget {
  const ScanGoWelcomeScreen({super.key});

  @override
  State<ScanGoWelcomeScreen> createState() => _ScanGoWelcomeScreenState();
}

class _ScanGoWelcomeScreenState extends State<ScanGoWelcomeScreen> {
  final _tenantCtrl = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _tenantCtrl.dispose();
    super.dispose();
  }

  Future<void> _startGuest() async {
    final s = context.read<LocaleController>().s;
    final code = _tenantCtrl.text.trim();
    if (code.isEmpty) {
      setState(() => _error = s.needStoreCode);
      return;
    }
    setState(() {
      _busy = true;
      _error = null;
    });
    try {
      final api = context.read<SelfCheckoutApi>();
      final session = await api.startGuest(tenantCode: code);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute<void>(
          builder: (_) => ScanGoCartScreen(session: session),
        ),
      );
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final locale = context.watch<LocaleController>();
    final s = locale.s;
    final theme = Theme.of(context);
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              Color(0xFFE8EEF8),
              ModernTechColors.ice,
              ModernTechColors.white,
            ],
          ),
        ),
        child: SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(24, 16, 24, 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Align(
                  alignment: AlignmentDirectional.centerEnd,
                  child: TextButton(
                    onPressed: () => locale.toggle(),
                    child: Text(s.languageToggle),
                  ),
                ),
                const Spacer(flex: 2),
                Text(
                  s.brand,
                  textAlign: TextAlign.center,
                  style: theme.textTheme.displaySmall?.copyWith(
                    color: ModernTechColors.royalBlue,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  s.guestTagline,
                  textAlign: TextAlign.center,
                  style: theme.textTheme.titleMedium?.copyWith(
                    color: ModernTechColors.muted,
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 40),
                TextField(
                  controller: _tenantCtrl,
                  textCapitalization: TextCapitalization.characters,
                  textInputAction: TextInputAction.done,
                  onSubmitted: (_) => _busy ? null : _startGuest(),
                  decoration: InputDecoration(
                    labelText: s.storeCode,
                    hintText: s.storeCodeHint,
                    prefixIcon: const Icon(Icons.storefront_outlined),
                  ),
                ),
                if (_error != null) ...[
                  const SizedBox(height: 12),
                  Text(
                    _error!,
                    textAlign: TextAlign.center,
                    style: TextStyle(color: theme.colorScheme.error),
                  ),
                ],
                const SizedBox(height: 20),
                FilledButton(
                  onPressed: _busy ? null : _startGuest,
                  child: _busy
                      ? const SizedBox(
                          height: 22,
                          width: 22,
                          child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                        )
                      : Text(s.continueGuest),
                ),
                const Spacer(flex: 3),
                TextButton(
                  onPressed: _busy
                      ? null
                      : () {
                          Navigator.of(context).push(
                            MaterialPageRoute<void>(
                              builder: (_) => const LoginScreen(),
                            ),
                          );
                        },
                  child: Text(s.staffEntry),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
