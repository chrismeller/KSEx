<?php

  ini_set('display_errors', true );
	error_reporting(-1);

	date_default_timezone_set( 'America/New_York' );

	$file = '/Users/chris/Downloads/ProgrammingTestDataSet.csv';

	$h = fopen( $file, 'r' );

	if ( !$h ) {
		die('Unable to open file');
	}

	$discarded = 0;
	$lines = array();
	while ( !feof( $h ) ) {

		$row = fgetcsv( $h );

		if ( empty( $row ) ) {
			continue;
		}

		if ( $row[1] == '' ) {
			$discarded++;
			continue;
		}

		$d = new DateTime( $row[2] );

		$lines[] = array(
			'PageName' => trim( $row[0] ),
			'CookieID' => $row[1],
			'VisitDateTime' => $row[2],
			'VisitTS' => $d->format('U'),
		);

	}

	fclose( $h );

	echo 'Discarded ' . $discarded . ' entries without a unique identifier' . "\n";

	// loop through all records and group them by customer, our first criteria
	$customers = array();
	foreach ( $lines as $line ) {
		if ( !isset( $customers[ $line['CookieID'] ] ) ) {
			$customers[ $line['CookieID'] ] = array();
		}

		$customers[ $line['CookieID'] ][] = $line;
	}

	$elapsed = array();
	foreach ( $customers as $customer => $transactions ) {

		$last_operation = null;
		$last_ts = null;
		foreach ( $transactions as $transaction ) {

			// this is to deal with the first row, which has some strange character encoding problems at the beginning
			// it's a hack, but i don't feel like setting up mb_string to convert and strip everything "properly"
			if ( strlen( $transaction['PageName'] ) == 10 ) {
				$transaction['PageName'] = substr( $transaction['PageName'], -7 );
			}

			if ( !isset( $elapsed[ $transaction['PageName'] ] ) ) {
				$elapsed[ $transaction['PageName'] ] = array();
			}

			// as long as this is not the first transaction, calculate the elapsed time
			if ( $last_operation != null ) {

				$elapsed_time = $transaction['VisitTS'] - $last_ts;

				$elapsed[ $last_operation ][] = $elapsed_time;

			}

			// save the last operation and timestamp
			$last_operation = $transaction['PageName'];
			$last_ts = $transaction['VisitTS'];

		}

	}

	foreach ( $elapsed as $type => $values ) {

		$total_time = array_sum( $values );

		$average = round( $total_time / count( $values ), 2 );

		echo 'Customers spent an average of ' . $average . ' seconds on ' . $type . ' pages' . "\n";

	}


?>
