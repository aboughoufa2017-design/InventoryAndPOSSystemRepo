import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// Modern Tech brand tokens from the POS specification (§5).
abstract final class ModernTechColors {
  static const Color royalBlue = Color(0xFF0A42A0);
  static const Color electricBlue = Color(0xFF0052FF);
  static const Color white = Color(0xFFFFFFFF);
  static const Color slate = Color(0xFF0F172A);
  static const Color ice = Color(0xFFF4F7FB);
  static const Color muted = Color(0xFF64748B);
}

ThemeData buildModernTechTheme() {
  final scheme = ColorScheme.fromSeed(
    seedColor: ModernTechColors.royalBlue,
    primary: ModernTechColors.royalBlue,
    secondary: ModernTechColors.electricBlue,
    surface: ModernTechColors.white,
    brightness: Brightness.light,
  );

  final textTheme = GoogleFonts.cairoTextTheme().apply(
    bodyColor: ModernTechColors.slate,
    displayColor: ModernTechColors.slate,
  );

  return ThemeData(
    useMaterial3: true,
    colorScheme: scheme,
    textTheme: textTheme,
    primaryTextTheme: textTheme,
    scaffoldBackgroundColor: ModernTechColors.ice,
    appBarTheme: AppBarTheme(
      backgroundColor: ModernTechColors.royalBlue,
      foregroundColor: ModernTechColors.white,
      elevation: 0,
      centerTitle: true,
      titleTextStyle: GoogleFonts.cairo(
        color: ModernTechColors.white,
        fontSize: 20,
        fontWeight: FontWeight.w700,
      ),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: ModernTechColors.royalBlue,
        foregroundColor: ModernTechColors.white,
        minimumSize: const Size.fromHeight(52),
        textStyle: GoogleFonts.cairo(fontSize: 16, fontWeight: FontWeight.w700),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: ModernTechColors.royalBlue,
        side: const BorderSide(color: ModernTechColors.royalBlue),
        minimumSize: const Size.fromHeight(52),
        textStyle: GoogleFonts.cairo(fontSize: 16, fontWeight: FontWeight.w600),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(
        foregroundColor: ModernTechColors.muted,
        textStyle: GoogleFonts.cairo(fontSize: 14, fontWeight: FontWeight.w600),
      ),
    ),
    listTileTheme: const ListTileThemeData(
      contentPadding: EdgeInsets.symmetric(horizontal: 12, vertical: 4),
    ),
    cardTheme: CardThemeData(
      color: ModernTechColors.white,
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: BorderSide(color: ModernTechColors.royalBlue.withValues(alpha: 0.12)),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: ModernTechColors.white,
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: BorderSide(color: ModernTechColors.royalBlue.withValues(alpha: 0.18)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: const BorderSide(color: ModernTechColors.electricBlue, width: 2),
      ),
      labelStyle: GoogleFonts.cairo(color: ModernTechColors.muted),
    ),
  );
}
