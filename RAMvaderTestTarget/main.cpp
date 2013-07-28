#include <iostream>
#include <iomanip>
#include <string>
#include <cstdlib>
#include <cassert>
#include <sstream>
#include <vector>


// STATIC GLOBALS
static char sg_Byte;
static short sg_Int16;
static int sg_Int32;
static long long sg_Int64;
static unsigned short sg_UInt16;
static unsigned int sg_UInt32;
static unsigned long long sg_UInt64;
static float sg_Single;
static double sg_Double;


// FUNCTIONS
/** Used in debug mode to check the sizes of the compiler's data types.
 * @param typeName The name of the type to be checked.
 * @param reportedSize The size reported by the compiler to the given type.
 * @param expectedSize The size expected for the given type.
 * @return Returns true if the reported size matches the expected size, false otherwise. */
bool dbgCheckDataTypeSize( const std::string &typeName, int reportedSize, int expectedSize )
{
	if ( reportedSize != expectedSize )
	{
		std::cout << "Type \"" << typeName << "\" has an invalid size! Expected size: "
			<< expectedSize << ". Reported Size: " << reportedSize << std::endl;
	}
	return (reportedSize == expectedSize);
}

/** Application's entry point.
 * @param argc The number of arguments for the argv parameter.
 * @param argv The arguments passed to the application. */
int main( int argc, char *argv[] )
{
	#ifdef _DEBUG
		// On debug mode, check the sizes of the data types.
		bool bDataTypesOk = true;

		bDataTypesOk &= dbgCheckDataTypeSize( "Byte (char)",                   sizeof( char ), 1 );
		bDataTypesOk &= dbgCheckDataTypeSize( "Int16 (short)",                 sizeof( short ), 2 );
		bDataTypesOk &= dbgCheckDataTypeSize( "Int32 (int)",                   sizeof( int ), 4 );
		bDataTypesOk &= dbgCheckDataTypeSize( "Int64 (long long)",             sizeof( long long ), 8 );
		bDataTypesOk &= dbgCheckDataTypeSize( "UInt16 (unsigned short)",       sizeof( unsigned short ), 2 );
		bDataTypesOk &= dbgCheckDataTypeSize( "UInt32 (unsigned int)",         sizeof( unsigned int ), 4 );
		bDataTypesOk &= dbgCheckDataTypeSize( "UInt64 (unsigned long long)",   sizeof( unsigned long long ), 8 );
		bDataTypesOk &= dbgCheckDataTypeSize( "Single (float)",                sizeof( float ), 4 );
		bDataTypesOk &= dbgCheckDataTypeSize( "Double (double)",               sizeof( double ), 8 );

		assert( bDataTypesOk );
	#endif

	// Introduction
	std::cout
		<< "Welcome! Type \"help\" to see the available options." << std::endl
		<< "NOTE: This is a CASE-SENSITIVE prompt." << std::endl;

	// Application's main loop
	bool bAppEnded = false;
	while ( bAppEnded == false )
	{
		// Read a line of command
		std::string cmdLine;
		std::cout << "> ";
		std::getline( std::cin, cmdLine );

		// Split the line in several arguments
		std::vector<std::string> cmdArgs;
		std::istringstream strSplitter( cmdLine );

		std::string curWord;
		while ( strSplitter >> curWord )
			cmdArgs.push_back( curWord );

		// Process commands
		if ( cmdArgs[0] == "help" )
		{
			std::cout
				<< "Available options:" << std::endl
				<< "print" << std::endl
				<< "   Prints all the available variables (type, address and value)." << std::endl
				<< "set {vartype} {value}" << std::endl
				<< "   Modify the value of a variable." << std::endl
				<< "   {vartype}: The type of variable you want to set." << std::endl
				<< "              Can be: Byte, Int16, Int32, Int64, UInt16, UInt32," << std::endl
				<< "                      UInt64, Single, Double." << std::endl
				<< "   {value}: The new value for the variable." << std::endl
				<< "exit" << std::endl
				<< "   Terminates the application." << std::endl;
		}
		else if ( cmdArgs[0] == "print" )
		{
			std::cout
				<< "[VARIABLE]   [VALUE]                [ADDRESS] " << std::endl
				<< "Byte         " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << (int)sg_Byte   << "   0x" << std::setfill( '0' ) << static_cast<void *>( &sg_Byte ) << std::endl
				<< "Int16        " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_Int16       << "   0x" << std::setfill( '0' ) << &sg_Int16  << std::endl
				<< "Int32        " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_Int32       << "   0x" << std::setfill( '0' ) << &sg_Int32  << std::endl
				<< "Int64        " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_Int64       << "   0x" << std::setfill( '0' ) << &sg_Int64  << std::endl
				<< "UInt16       " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_UInt16      << "   0x" << std::setfill( '0' ) << &sg_UInt16 << std::endl
				<< "UInt32       " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_UInt32      << "   0x" << std::setfill( '0' ) << &sg_UInt32 << std::endl
				<< "UInt64       " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_UInt64      << "   0x" << std::setfill( '0' ) << &sg_UInt64 << std::endl
				<< "Single       " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_Single      << "   0x" << std::setfill( '0' ) << &sg_Single << std::endl
				<< "Double       " << std::setfill( ' ' ) << std::setw( 20 ) << std::left << sg_Double      << "   0x" << std::setfill( '0' ) << &sg_Double << std::endl;
		}
		else if ( cmdArgs[0] == "set" )
		{
			// Verify arguments
			if ( cmdArgs.size() < 3 )
			{
				std::cerr << "Incorrect number of arguments!" << std::endl;
				continue;
			}

			const std::string &varName = cmdArgs[1];

			bool bIsByte = false, bIsInt16 = false, bIsInt32 = false, bIsInt64 = false,
				bIsUInt16 = false, bIsUInt32 = false, bIsUInt64 = false,
				bIsSingle = false, bIsDouble = false;

			if ( varName == "Byte" )
				bIsByte = true;
			else if ( varName == "Int16" )
				bIsInt16 = true;
			else if ( varName == "Int32" )
				bIsInt32 = true;
			else if ( varName == "Int64" )
				bIsInt64 = true;
			else if ( varName == "UInt16" )
				bIsUInt16 = true;
			else if ( varName == "UInt32" )
				bIsUInt32 = true;
			else if ( varName == "UInt64" )
				bIsUInt64 = true;
			else if ( varName == "Single" )
				bIsSingle = true;
			else if ( varName == "Double" )
				bIsDouble = true;
			else
			{
				std::cerr << "Incorrect variable name!" << std::endl;
				continue;
			}

			// Update values
			std::istringstream valueExtractor( cmdArgs[2] );
			bool bFailedToRead = false;
			if ( bIsByte || bIsInt16 || bIsInt32 || bIsInt64 )
			{
				long long val;
				valueExtractor >> val;
				bFailedToRead = valueExtractor.fail();
				if ( bFailedToRead == false )
				{
					if ( bIsByte )
						sg_Byte = static_cast<char>( val );
					else if ( bIsInt16 )
						sg_Int16 = static_cast<short>( val );
					else if ( bIsInt32 )
						sg_Int32 = static_cast<int>( val );
					else if ( bIsInt64 )
						sg_Int64 = static_cast<long long>( val );
				}
			}
			else if ( bIsUInt16 || bIsUInt32 || bIsUInt64 )
			{
				unsigned long long val;
				valueExtractor >> val;
				bFailedToRead = valueExtractor.fail();
				if ( bFailedToRead == false )
				{
					if ( bIsUInt16 )
						sg_UInt16 = static_cast<unsigned short>( val );
					else if ( bIsUInt32 )
						sg_UInt32 = static_cast<unsigned int>( val );
					else if ( bIsUInt64 )
						sg_UInt64 = static_cast<unsigned long long>( val );
				}
			}
			else if ( bIsSingle || bIsDouble )
			{
				double val;
				valueExtractor >> val;
				bFailedToRead = valueExtractor.fail();
				if ( bFailedToRead == false )
				{
					if ( bIsSingle )
						sg_Single = static_cast<float>( val );
					else if ( bIsDouble )
						sg_Double = static_cast<double>( val );
				}
			}

			// Print errors
			if ( bFailedToRead )
			{
				std::cout << "Could not read the value \"" << cmdArgs[2] << "\" and cast it to type \"" << cmdArgs[1] << "\"." << std::endl;
				continue;
			}
		}
		else if ( cmdArgs[0] == "exit" )
			bAppEnded = true;
		else
			std::cerr << "Unrecognized command: " << cmdArgs[0] << "." << std::endl << "Type 'help' if you need to see the available options." << std::endl;
	}


	// Exit
	return EXIT_SUCCESS;
}
